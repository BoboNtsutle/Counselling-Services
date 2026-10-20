// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Small UI helpers and Bootstrap animations/enhancements
document.addEventListener('DOMContentLoaded', function () {
    // Auto-dismiss flash messages after 6 seconds (Bootstrap alert + fade)
    setTimeout(function () {
        document.querySelectorAll('.flash.alert-dismissible').forEach(function (el) {
            var bsAlert = bootstrap.Alert.getOrCreateInstance(el);
            bsAlert.close();
        });
    }, 6000);

    // Simple reveal animation for card-a elements when they scroll into view
    var cards = document.querySelectorAll('.card-a');
    var observer = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            if (entry.isIntersecting) {
                entry.target.classList.add('reveal');
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.12 });
    cards.forEach(function (c) { observer.observe(c); });

    // Show skeleton during announcement publish for admins
    var publishForm = document.getElementById('publishForm');
    if (publishForm) {
        publishForm.addEventListener('submit', function () {
            var skl = document.getElementById('publishSkeleton');
            if (skl) skl.style.display = 'block';
        });
    }
});
