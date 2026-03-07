@ECHO OFF
setlocal enabledelayedexpansion
for %%f in (*.zip) do (
  echo Y | gh release delete-asset v1.0.0 %%f
  echo Y | gh release upload v1.0.0 %%f
)
for %%f in (*.json) do (
  echo Y | gh release delete-asset v1.0.0 %%f
  echo Y | gh release upload v1.0.0 %%f
)