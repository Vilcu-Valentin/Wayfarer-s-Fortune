# Camera

A Unity Camera defined by a Transform, field of view, near-plane, far-plane, aspect ratio, resolution. You can also access the color and depth buffer.

## Properties

| **Property**                | **Description**                                                                                                                                                                                                                                                                                      |
|-----------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Transform**               | The transform of the camera. This is a [Transform](Type-Transform.md) that contains the camera's position and orientation.                                                                                                                                                                           |
| **Orthographic**            | Is the camera orthographic (true) or perspective (false)? For more information, see [orthographic](https://docs.unity3d.com/ScriptReference/Camera-orthographic.html).                                                                                                                               |
| **Field Of View**           | The height of the camera's view angle (in degrees) along the local y-axis. For more information, see [fieldOfView](https://docs.unity3d.com/ScriptReference/Camera-fieldOfView.html).                                                                                                                |
| **Near Plane**              | The closest plane, relative to the camera, at which drawing occurs. For more information, see [nearPlane](https://docs.unity3d.com/ScriptReference/Camera-nearClipPlane.html).                                                                                                                       |
| **Far Plane**               | The furthest plane, relative to the camera, at which drawing occurs. For more information, see [farPlane](https://docs.unity3d.com/ScriptReference/Camera-farClipPlane.html).                                                                                                                        |
| **Orthographic Size**       | The camera's half-size when in orthographic mode. For more information, see [orthographicSize](https://docs.unity3d.com/ScriptReference/Camera-orthographicSize.html).                                                                                                                               |
| **Aspect Ratio**            | The proportional relationship between the camera's width and height. For more information, see [aspect](https://docs.unity3d.com/ScriptReference/Camera-aspect.html).                                                                                                                                |
| **Pixel Dimensions**        | The width and height of the camera in pixels, before dynamic resolution scaling. For more information, see [pixelWidth](https://docs.unity3d.com/ScriptReference/Camera-pixelWidth.html) and [pixelHeight](https://docs.unity3d.com/ScriptReference/Camera-pixelHeight.html).                        |
| **Scaled Pixel Dimensions** | The width and height of the camera in pixels, after dynamic resolution scaling. For more information, see [scaledPixelWidth](https://docs.unity3d.com/ScriptReference/Camera-scaledPixelWidth.html) and [scaledPixelHeight](https://docs.unity3d.com/ScriptReference/Camera-scaledPixelHeight.html). |
| **Lens Shift**              | The lens offset of the camera. For more information, see [lensShift](https://docs.unity3d.com/ScriptReference/Camera-lensShift.html).                                                                                                                                                                |
| **Depth Buffer**            | Specifies the depth buffer for this camera.                                                                                                                                                                                                                                                          |
| **Color Buffer**            | Specifies the color buffer for this camera.                                                                                                                                                                                                                                                          |