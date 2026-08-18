# Deucarian Viewer Shell

`com.deucarian.viewer-shell` is the reusable, consumer-neutral shell used by
Deucarian 3D viewers. It provides the shared status toast, diagnostics menu,
display-settings menu, responsive screen layout, theme composition, and input
guard hooks. Products supply lifecycle state and the shared Viewer Rendering
controller; they do not rebuild the shell UI.

Current package version: `0.1.0`. Unity `6000.0` or newer is required.

## Ownership

This package owns:

- the reference viewer-shell profile and composition API;
- loading, ready-toast, and error status presentation;
- the developer diagnostics body and consumer-neutral diagnostics text;
- the standard Color faithful / Realistic display-settings view;
- coordination of the two top-right menus through the package-owned UI menu
  cluster;
- responsive shell constants and exact reference motion timings.

It does not own navigation, model loading, rendering implementation, report or
activity data, markers, media, backend commands, authentication, or product
branding. Consumers adapt those capabilities to the contracts in this package.

## Install

```json
{
  "dependencies": {
    "com.deucarian.viewer-shell": "https://github.com/Deucarian/Viewer-Shell.git#main"
  }
}
```

Use `#develop` for development builds.

## Reference composition

Create one theme provider for the whole viewer, then install the shell with an
adapter for the viewer's rendering implementation:

```csharp
DeucarianThemeProvider themeProvider =
    ViewerShellReferenceComposition.EnsureThemeProvider(viewerRoot);

ViewerShellConfiguration configuration =
    ViewerShellReferenceComposition.CreateConfiguration(
        themeProvider,
        shouldAnimate: () => true,
        bindInputGuard: root => navigationInputGuard.Bind(root));

ViewerShellPresenter shell = ViewerShellReferenceComposition.Install(
    viewerRoot.transform,
    displaySettingsController,
    configuration);

shell.ApplyStatus(ViewerShellStatusSnapshot.Loading(
    "Preparing the model...",
    "model=example"));
```

`BindInputGuard` is deliberately consumer-supplied. Viewer Shell therefore
shares the exact interaction boundary without taking a dependency on any
specific navigation/input package.

## Rendering integration

Pass the authoritative
`Deucarian.ViewerRendering.IViewerRenderingController`. The shell reads its
`CurrentSettings`, listens to `SettingsChanged`, and sends
`ViewerDisplaySettingsRequest` values tagged as `ViewerUi` when a person uses
the menu. Rendering is implemented once by `com.deucarian.viewer-rendering`;
browser commands and product events remain consumer-owned.

## Layering and tooltips

Viewer Shell never assigns numeric UI depths or creates `PanelSettings`.
Status, menu, and tooltip surfaces are configured through `com.deucarian.ui`.
That package remains the single screen-space layering authority, so tooltips
always render above shell menus and other viewer UI.

## Validation

Run the package validator and Unity EditMode tests when changing runtime code:

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
```

## License

See [LICENSE.md](LICENSE.md).
