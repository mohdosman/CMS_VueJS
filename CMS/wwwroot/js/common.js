/* [ ---- common ---- ] */

	//* detect touch devices
    function is_touch_device() {
	  return !!('ontouchstart' in window);
	}

	document.addEventListener('DOMContentLoaded', function() {

		//* main menu mouseover
		nav_mouseover.init();
		//* top submenu
		submenu.init();

		//* mobile navigation
		selectnav('mobile-nav', {
			indent: '-'
		});

		document.body.addEventListener('touchstart', function (e) {
			if (e.target.closest('.dropdown-menu')) e.stopPropagation();
		});

	});


	//* main menu mouseover
	nav_mouseover = {
		init: function() {
			function setExpanded(li, toggle, expanded) {
				li.classList.toggle('navHover', expanded);
				if (!expanded) li.classList.remove('open');
				if (toggle) toggle.setAttribute('aria-expanded', expanded ? 'true' : 'false');
			}

			document.querySelectorAll('header li.dropdown').forEach(function (li) {
				// Not tag-qualified: the toggle is a real <button> now, not the
				// <a href="javascript:void(0)" role="button"> it used to be — WAVE
				// flagged those as redundant links since every toggle shared that href.
				var toggle = li.querySelector(':scope > .dropdown-toggle');

				li.addEventListener('mouseenter', function() {
					if (document.body.classList.contains('menu_hover')) {
						setExpanded(li, toggle, true);
					}
				});
				li.addEventListener('mouseleave', function() {
					if (document.body.classList.contains('menu_hover')) {
						setExpanded(li, toggle, false);
					}
				});

				// Bootstrap's JS (which handled this) isn't loaded, and mouseenter/mouseleave
				// alone leave the submenu unreachable without a pointer (WCAG 2.1.1 Keyboard).
				//
				// This toggle is role="button" revealing a plain <ul>/<a> list, not an ARIA
				// menu (role="menu"/"menuitem") — that's the Disclosure pattern, not Menu
				// Button. Per the ARIA APG, a disclosure's keyboard model is Enter/Space to
				// toggle, then Tab through whatever became visible like any other links; it
				// has no Up/Down roving-focus navigation between items, unlike a real menu.
				// ArrowDown-to-open-and-focus-first-item below is the one bit menus and
				// disclosures agree on. Don't add Up/Down-between-items here without also
				// adding role="menu"/"menuitem" + roving tabindex and changing Tab to close
				// the menu instead of walking through it — mixing the two patterns halfway
				// is worse than either one done fully.
				if (toggle) {
					toggle.addEventListener('keydown', function (e) {
						if (e.key === 'Enter' || e.key === ' ' || e.key === 'Spacebar') {
							e.preventDefault();
							setExpanded(li, toggle, !li.classList.contains('navHover'));
						} else if (e.key === 'ArrowDown' && !li.classList.contains('navHover')) {
							e.preventDefault();
							setExpanded(li, toggle, true);
							var firstItem = li.querySelector('.dropdown-menu a, .dropdown-menu button');
							if (firstItem) firstItem.focus();
						}
					});
				}

				// Escape, or tabbing out of the item entirely, closes it — the keyboard
				// equivalents of mouseleave, which a keyboard user never triggers.
				li.addEventListener('keydown', function (e) {
					if (e.key === 'Escape' && li.classList.contains('navHover')) {
						setExpanded(li, toggle, false);
						if (toggle) toggle.focus();
					}
				});
				li.addEventListener('focusout', function (e) {
					if (!li.contains(e.relatedTarget)) setExpanded(li, toggle, false);
				});
			});

			// Bootstrap's JS isn't loaded, so there is no clearMenus: without this the
			// menu only closes when the pointer happens to leave it. Buttons (theme
			// swatches) are excluded so picking a theme keeps the menu open.
			var header = document.querySelector('header');
			if (header) {
				header.addEventListener('click', function (e) {
					var link = e.target.closest('.dropdown-menu a');
					if (!link) return;
					var li = link.closest('li.dropdown');
					if (!li) return;
					var toggle = li.querySelector(':scope > .dropdown-toggle');
					setExpanded(li, toggle, false);
				});
			}
		}
	};

	//* submenu
	submenu = {
		init: function() {
			document.querySelectorAll('.dropdown-menu li').forEach(function (li) {
				var childUl = li.querySelector(':scope > ul');
				if (childUl) {
					li.classList.add('sub-dropdown');
					childUl.classList.add('sub-menu');
				}
			});

			document.querySelectorAll('.sub-dropdown').forEach(function (li) {
				var childUl = li.querySelector(':scope > ul');
				li.addEventListener('mouseenter', function () {
					li.classList.add('active');
					if (childUl) childUl.classList.add('sub-open');
				});
				li.addEventListener('mouseleave', function () {
					li.classList.remove('active');
					if (childUl) childUl.classList.remove('sub-open');
				});
			});

		}
	};

