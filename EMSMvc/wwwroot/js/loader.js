(function () {
    const LOADER_DELAY_MS = 1500;
    const loaderOverlay = document.getElementById('page-loader-overlay');

    function hideLoader() {
        if (loaderOverlay) {
            loaderOverlay.classList.add('hidden');
        }
        document.body.classList.remove('content-hidden-during-load');
    }

    if (loaderOverlay) {
        setTimeout(hideLoader, LOADER_DELAY_MS);
    } else {
        document.body.classList.remove('content-hidden-during-load');
    }
})();
