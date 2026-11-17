// Closes a notice for the rest of the visit. The notice needs
// data-permanent, or the next enhanced navigation brings it back.
document.addEventListener('click', event => {
  const button = event.target.closest('[data-close-notice]');
  if (button) {
    button.closest('.notice').hidden = true;
  }
});
