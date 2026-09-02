(function () {
  var root = document.documentElement;
  var toggles = Array.prototype.slice.call(document.querySelectorAll("[data-lf-theme-toggle]"));
  var sunIcon = '<svg viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="12" r="4"></circle><path d="M12 2v2M12 20v2M4.93 4.93l1.41 1.41M17.66 17.66l1.41 1.41M2 12h2M20 12h2M4.93 19.07l1.41-1.41M17.66 6.34l1.41-1.41"></path></svg>';
  var moonIcon = '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M20.5 14.2A7.8 7.8 0 0 1 9.8 3.5a8.7 8.7 0 1 0 10.7 10.7Z"></path></svg>';

  function currentTheme() {
    return root.getAttribute("data-bs-theme") || "light";
  }

  function applyTheme(theme) {
    root.setAttribute("data-bs-theme", theme);
    toggles.forEach(function (toggle) {
      var icon = toggle.querySelector("[data-lf-theme-icon]");
      if (icon) {
        icon.innerHTML = theme === "dark" ? sunIcon : moonIcon;
      }

      var label = theme === "dark" ? "Açık temaya geç" : "Koyu temaya geç";
      toggle.setAttribute("aria-label", label);
      toggle.setAttribute("title", label);
      toggle.setAttribute("aria-pressed", theme === "dark" ? "true" : "false");
    });
  }

  applyTheme(currentTheme());

  toggles.forEach(function (toggle) {
    toggle.addEventListener("click", function () {
      var nextTheme = currentTheme() === "dark" ? "light" : "dark";
      localStorage.setItem("leaveflow-theme", nextTheme);
      applyTheme(nextTheme);
    });
  });
})();
