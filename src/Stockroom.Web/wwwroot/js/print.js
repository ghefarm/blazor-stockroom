// Called through JS interop by the PrintButton component.
export function printPage(title) {
  // Browsers offer the title as the file name for a PDF.
  const previous = document.title;
  document.title = title;
  window.print();
  document.title = previous;
}
