window.scrollTimelineToHour = (hour, totalWidth) => {
    const el = document.getElementById("timelineScroll");
    if (!el) return;

    const pxPerHour = totalWidth / 24;
    const target = pxPerHour * hour;

    el.scrollTo({
        left: target,
        behavior: "smooth"
    });
};