# Deucarian Viewer Shell Agent Notes

Package ID: `com.deucarian.viewer-shell`

Follow the canonical Deucarian governance documents in Package Registry.

## Ownership

This package owns the reusable viewer-shell experience: reference shell
composition, status/toast presentation, diagnostics presentation, standard
display-settings UI contracts, responsive shell layout, and coordination of
the two canonical top-right viewer menus.

It must not own camera navigation, pointer-lock implementation, raw input,
model loading, rendering implementation, commands/transports, authentication,
report/activity DTOs, markers, media, product branding, or WebGL host markup.

## Dependencies

- Common owns the easing primitive used by the ready-toast fade.
- Theming owns theme families, semantic color roles, typography, and visual
  styles.
- UI owns UI Toolkit primitives, glass, morphing menu chrome, semantic depth,
  overlay hosts, and tooltips.
- Viewer Rendering owns the authoritative display-settings snapshot, request,
  mode, source, and controller contracts consumed by the settings body.
- Unity UIElements supplies runtime UI Toolkit types.
- Navigation is intentionally not a dependency. Consumers inject an input
  guard callback into `ViewerShellConfiguration`.

Do not duplicate Viewer Rendering display contracts. Do not add Navigation,
Diagnostics, Logging, or another package unless
production code truly needs its owned capability and all ecosystem metadata is
updated together.

## Policies

- Keep all public contracts consumer-neutral; never add Report or Activity
  names, data, commands, or formatting.
- Do not create or ship `PanelSettings`, assign numeric screen-space depths,
  or implement another tooltip layer.
- Do not duplicate generic morphing-menu chrome or menu-cluster coordination
  from UI.
- Do not add direct `UnityEngine.Debug` calls.
- Do not add direct runtime object-destruction helpers. Test teardown may use
  `DestroyImmediate`.
- Keep runtime files below 500 lines.
- Preserve one injected theme provider across the entire viewer composition.

## Validation

Run:

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
```

Also run EditMode tests and `git diff --check`.
