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

window.getTimelineOffset = (headerId) => {
    const header = document.getElementById(headerId);
    const wrapper = document.getElementById("timelineScroll");

    const rect = header.getBoundingClientRect();

    return {
        left: rect.left,
        width: rect.width,
        scrollLeft: wrapper.scrollLeft
    };
};