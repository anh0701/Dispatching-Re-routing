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

window.getTimelineOffset = (id) => {
    const el = document.getElementById(id);
    if (!el) return 0;
    return el.getBoundingClientRect().left;
};