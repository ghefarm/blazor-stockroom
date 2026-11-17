// One listener on the document serves every copy button, also
// on pages that arrive later through enhanced navigation.
document.addEventListener('click', async event => {
  const button = event.target.closest('button[data-copy]');
  if (!button) {
    return;
  }

  const source = document.getElementById(button.dataset.copy);
  if (!source) {
    return;
  }

  // The clipboard API exists only on HTTPS and on localhost.
  if (window.isSecureContext && navigator.clipboard) {
    try {
      await navigator.clipboard.writeText(source.textContent);
      showState(button, 'copied');
      return;
    } catch {
      // Blocked by the browser: selecting the text still helps.
    }
  }

  selectText(source);
  showState(button, 'selected');
});

function selectText(element) {
  const range = document.createRange();
  range.selectNodeContents(element);
  const selection = window.getSelection();
  selection.removeAllRanges();
  selection.addRange(range);
}

// The text "Copied" comes from CSS. The script only sets an
// attribute, and never touches the content of the button.
function showState(button, state) {
  button.dataset.copyState = state;
  setTimeout(() => delete button.dataset.copyState, 2000);
}
