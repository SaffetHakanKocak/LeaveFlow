(function () {
  var root = document.documentElement;
  var toggle = document.querySelector("[data-lf-theme-toggle]");
  var icon = document.querySelector("[data-lf-theme-icon]");

  function currentTheme() {
    return root.getAttribute("data-bs-theme") || "light";
  }

  function applyTheme(theme) {
    root.setAttribute("data-bs-theme", theme);
    if (icon) {
      icon.textContent = theme === "dark" ? "☾" : "☼";
    }
    if (toggle) {
      toggle.setAttribute("aria-pressed", theme === "dark" ? "true" : "false");
    }
  }

  applyTheme(currentTheme());

  if (!toggle) {
    return;
  }

  toggle.addEventListener("click", function () {
    var nextTheme = currentTheme() === "dark" ? "light" : "dark";
    localStorage.setItem("leaveflow-theme", nextTheme);
    applyTheme(nextTheme);
  });
})();
