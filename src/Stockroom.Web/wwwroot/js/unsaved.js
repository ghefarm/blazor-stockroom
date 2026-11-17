// Forms marked with data-warn-unsaved ask before a link leaves
// the page. The snapshot is taken at the first touch, so the
// values the server filled in count as unchanged.
let snapshots = new WeakMap();

function readForm(form) {
  // FormData includes hidden fields. They can change as well.
  return new URLSearchParams(new FormData(form)).toString();
}

function hasChanges() {
  const forms = document.querySelectorAll('form[data-warn-unsaved]');
  return [...forms].some(form =>
    snapshots.has(form) && snapshots.get(form) !== readForm(form));
}

document.addEventListener('focusin', event => {
  const form = event.target.closest('form[data-warn-unsaved]');
  if (form && !snapshots.has(form)) {
    snapshots.set(form, readForm(form));
  }
});

document.addEventListener('submit', event => {
  snapshots.delete(event.target);
});

// Capture phase: Blazor handles link clicks on the document as
// well, and this check must run before a navigation starts.
document.addEventListener('click', event => {
  const link = event.target.closest('a[href]');
  if (!link || !hasChanges()) {
    return;
  }

  if (!confirm('You have unsaved changes. Leave this page?')) {
    event.preventDefault();
    event.stopPropagation();
  }
}, true);

// After an enhanced navigation the old forms are gone, or they
// show new values from the server. Start without snapshots.
Blazor.addEventListener('enhancedload', () => {
  snapshots = new WeakMap();
});
