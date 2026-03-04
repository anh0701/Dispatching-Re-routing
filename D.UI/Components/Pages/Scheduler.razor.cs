using D.UI.Components.Pages;
using D.UI.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using MudBlazor;

namespace D.UI.Pages
{
    public partial class Scheduler : ComponentBase
    {
        [Inject] public WorkOrderService WorkOrderService { get; set; } = default!;
        [Inject] public IDialogService DialogService { get; set; } = default!;
        [Inject] public ISnackbar Snackbar { get; set; } = default!;
        [Inject] public IJSRuntime JS { get; set; } = default!;
        [Inject] public HttpClient Http { get; set; } = default!;
        [Inject] public NavigationManager Navigation { get; set; } = default!;

        protected List<JobItem>? _jobs;
        protected List<Machine>? _machines;
        protected List<DispatchTaskDto> _board = new();
        protected DateTime _selectedDate = DateTime.Today;

        protected List<int> _hours = Enumerable.Range(0, 24).ToList();
        protected JobItem? _draggingJob;
        protected bool _isDragging;
        protected double _mouseX;
        protected double _mouseY;
        protected int? _hoverMachineId;

        protected const double TimelineWidthPx = 2400;
        protected bool _scrolled;

        protected Dictionary<int, HashSet<int>> _jobMachineMap = new();
        private string? _myConnectionId;
        private HubConnection? hubConnection;


        protected override async Task OnInitializedAsync()
        {
            try
            {
                _jobs = await WorkOrderService.GetPendingJobsAsync();
                _machines = await WorkOrderService.GetWorkCentersAsync();
                await PreloadJobCapabilities();
                await LoadBoardOnly();
                Console.WriteLine("Load 1");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Init error: {ex}");
            }

            hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5096/dispatchHub")
                .WithAutomaticReconnect()
                .Build();

            hubConnection.On<int>("ReceiveUpdate", async (dispatchId) =>
            {
                try
                {
                    if (_isDragging) return;
                    await InvokeAsync(LoadBoardOnly);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ReceiveUpdate error: {ex}");
                }
            });

            try
            {
                await hubConnection.StartAsync();
                _myConnectionId = hubConnection.ConnectionId;
                Console.WriteLine("SignalR connected");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR Connection Error: {ex}");
            }

            hubConnection.Closed += async (error) =>
            {
                Console.WriteLine($"SignalR closed: {error}");
                await Task.Delay(2000);
            };
          
        }

        public async ValueTask DisposeAsync()
        {
            if (hubConnection != null)
            {
                await hubConnection.DisposeAsync();
            }
        }

        protected string GetMachineColor(int status) => status switch
        {
            0 => "status-offline",
            1 => "status-running",
            2 => "status-maint",
            3 => "status-break",
            _ => ""
        };

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && !_scrolled)
            {
                _scrolled = true;

                await JS.InvokeVoidAsync(
                    "scrollTimelineToHour",
                    8,
                    TimelineWidthPx
                );
            }
        }

        private bool _loadingBoard;
        protected async Task LoadBoardOnly()
        {
            if (_loadingBoard) return;
            _loadingBoard = true;

            try
            {
                var board = await WorkOrderService.GetBoardAsync(_selectedDate);
                _board = board ?? new List<DispatchTaskDto>();
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadBoardOnly error: {ex}");
            }
            finally
            {
                _loadingBoard = false;
            }
        }

        protected async Task OnDateChanged(DateTime? date)
        {
            if (date is null) return;
            _selectedDate = date.Value;
            await LoadBoardOnly();
        }

        protected double GetLeftPercent(DateTime? start)
        {
            if (start == null) return 0;

            var baseDate = _selectedDate.Date;
            var span = start.Value - baseDate;

            var minutes = span.TotalMinutes;

            var px = minutes / (24 * 60) * TimelineWidthPx;

            return px / TimelineWidthPx * 100;
        }

        protected double GetWidthPercent(DateTime? start, DateTime? end)
        {
            if (start == null || end == null) return 2;

            var span = end.Value - start.Value;
            var minutes = span.TotalMinutes;

            var px = minutes / (24 * 60) * TimelineWidthPx;

            return Math.Max(1, px / TimelineWidthPx * 100);
        }

        protected void StartDrag(MouseEventArgs e, JobItem job)
        {
            _draggingJob = job;
            _isDragging = true;
            _mouseX = e.ClientX;
            _mouseY = e.ClientY;
        }

        protected void OnMouseMove(MouseEventArgs e)
        {
            if (!_isDragging) return;
            _mouseX = e.ClientX;
            _mouseY = e.ClientY;
        }

        protected async Task<DateTime> GetDropTimeAsync(double clientX)
        {
            var info = await JS.InvokeAsync<TimelineInfo>(
                "getTimelineOffset",
                "timeHeader"
            );

            var offsetX = clientX - info.left + info.scrollLeft;

            offsetX = Math.Clamp(offsetX, 0, info.width);

            var minutes = offsetX / info.width * (24 * 60);

            return _selectedDate.Date.AddMinutes(minutes);
        }

        protected async Task OnMouseUp(MouseEventArgs e)
        {
            if (!_isDragging) return;

            if (_hoverMachineId.HasValue && _draggingJob != null)
            {
                var route = await WorkOrderService.GetRouteDetailsAsync(_draggingJob.Id);

                Console.WriteLine("Detail: " + string.Join(", ", route));

                var op = route?.Steps?
                    .OrderBy(x => x.StepOrder)
                    .FirstOrDefault();

                if (op is null)
                {
                    Snackbar.Add("Không có máy khả dụng", Severity.Error);
                }
                else
                {
                    var scheduledStart = await GetDropTimeAsync(e.ClientX);
                    var result = await WorkOrderService.AssignJobAsync(
                        _draggingJob.Id,
                        _hoverMachineId.Value,
                        op.OperationId,
                        op.StepOrder,
                        scheduledStart.ToUniversalTime());

                    if (result.ok)
                    {
                        Snackbar.Add("Gán lệnh thành công", Severity.Success);
                        await LoadBoardOnly();
                    }
                    else
                    {
                        Snackbar.Add(
                        result.error ?? "Không thể gán lệnh",
                        Severity.Error,
                        config =>
                        {
                            config.VisibleStateDuration = 6000;
                            config.RequireInteraction = false;
                            config.ShowCloseIcon = true;
                        });
                    }
                }
            }

            _isDragging = false;
            _draggingJob = null;
            _hoverMachineId = null;

            StateHasChanged();
        }

        protected async Task OpenJobDialog(JobItem job)
        {
            var parameters = new DialogParameters
            {
                ["Job"] = job
            };

            var options = new DialogOptions
            {
                CloseButton = true,
                MaxWidth = MaxWidth.Small,
                FullWidth = true
            };

            var dialog = await DialogService.ShowAsync<JobDetailDialog>(
                "Cấu hình lệnh",
                parameters,
                options);

            var result = await dialog.Result;

            if (!result.Canceled)
            {
                await LoadBoardOnly();
                StateHasChanged();
            }
        }

        protected async Task PreloadJobCapabilities()
        {
            if (_jobs == null) return;

            foreach (var job in _jobs)
            {
                var route = await WorkOrderService.GetRouteDetailsAsync(job.Id);

                var machines = route?.Steps?
                    .SelectMany(s => s.AvailableMachines ?? [])
                    .Select(m => m.workCenterId)
                    .ToHashSet()
                    ?? new HashSet<int>();

                Console.WriteLine($"Job {job.Id} steps: {route?.Steps?.Count}");

                foreach (var step in route?.Steps ?? [])
                {
                    Console.WriteLine($"Step {step.StepOrder} machines: {step.AvailableMachines?.Count}");
                }
                _jobMachineMap[job.Id] = machines;
            }

            foreach (var kv in _jobMachineMap)
            {
                Console.WriteLine($"Job {kv.Key}: {string.Join(",", kv.Value)}");
            }
        }

        // @* check khi machine hover *@
        protected bool IsJobRunnable(JobItem job)
        {
            if (!_hoverMachineId.HasValue) return false;

            if (!_jobMachineMap.TryGetValue(job.Id, out var machines))
                return false;

            return machines.Contains(_hoverMachineId.Value);
        }

        // @* co AvailableMachines thi hien thi mau xanh luon *@
        protected bool HasAvailableMachine(JobItem job)
        {
            if (!_jobMachineMap.TryGetValue(job.Id, out var machines))
                return false;
            return machines.Count > 0;
        }
    }
}