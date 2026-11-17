// Prints one part of the page in a window of its own, so the print
// dialog never opens in the tab that holds the Blazor circuit.
export function printPage(title, selector) {
  const source = document.querySelector(selector);
  if (!source) {
    return;
  }

  const win = window.open("", "_blank", "width=1100,height=800");
  if (!win) {
    // A popup blocker stopped the window. Print the tab instead.
    window.print();
    return;
  }

  win.document.write(createDocument(source.outerHTML));
  win.document.close();
  // The title becomes the file name of "Save as PDF".
  win.document.title = title;
  fitCharts(win.document);

  win.addEventListener("load", () => {
    win.addEventListener("afterprint", () => win.close());
    win.focus();
    win.print();
  });
}

// Links, not copied rules: the load event of the new document
// waits for linked stylesheets, so printing starts after them.
function createDocument(html) {
  const links = [...document.querySelectorAll("link[rel=stylesheet]")]
    .map(link => `<link rel="stylesheet" href="${link.href}">`)
    .join("");

  return "<!DOCTYPE html><html><head><meta charset=\"utf-8\">"
    + links + "</head><body class=\"print-window\">"
    + html + "</body></html>";
}

// The chart writes its screen width in pixels into the SVG and has
// no viewBox. On narrower paper the right part would be cut off.
function fitCharts(doc) {
  for (const svg of doc.querySelectorAll("svg.apexcharts-svg")) {
    const width = svg.getAttribute("width");
    const height = svg.getAttribute("height");
    svg.setAttribute("viewBox", `0 0 ${width} ${height}`);
    svg.setAttribute("width", "100%");
    svg.style.height = "auto";
  }

  // The box around the SVG has the same pixel size in its style.
  for (const box of doc.querySelectorAll(".apexcharts-canvas")) {
    box.style.width = "100%";
    box.style.height = "auto";
    box.style.minHeight = "0";
  }

  for (const legend of doc.querySelectorAll(".apexcharts-legend")) {
    legend.style.position = "static";
  }
}
