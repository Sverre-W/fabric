# Elsa Harness

Standalone static page for reproducing Elsa custom element rendering issues without the main frontend app.

## Files

- `index.html`: page shell + config form
- `main.js`: asset boot, Blazor start, editor mount
- `styles.css`: local page layout

## Required local assets

This folder expects these paths to exist under the same HTTP root:

- `_content/`
- `_framework/`
- `appsettings.json`

They can be copied or symlinked from `src/frontend/public/`.

`Elsa.Studio.Host.CustomElements.styles.css` is optional in this repo snapshot. A local placeholder file is included.

## Run

From `src/elsa/`:

```bash
python -m http.server 8000
```

Open:

```text
http://localhost:8000
```

Optional query params:

```text
http://localhost:8000?backend=https://localhost:5001/elsa/api&definitionId=your-definition-id&token=your-access-token
```

The form values are also stored in `localStorage`.

## Notes

- Uses `elsa-workflow-definition-editor`
- Expects Elsa backend auth via bearer access token
- If assets fail to load, check browser network tab for missing `_content` or `_framework` files
