# Rendering

This article describes how the rendering is done in this project.

## 3D
Drawing a whole block is bad. You can possibly only see three of all faces, drawing six is irresponsibly expensive. For most blocks, they won't be visible at all.
It seems that providing a unified way to render block*faces* is better overall and should be used. A disadvantage is the non-reusability of vertices. That's ok, using the same vertex while interpolating for different textures on the same block seems impossible to do right.

A face can be identified by the position of a block and its direction. It needs to have meaningful texture coordinates (which should also be usable with a texture atlas).

However, we can't use the geometry shader, since the output of the vertex shader is in NDC. Generating new primitives in NDC seems to be impossible.

So we now know: The vertex shader needs four inputs for each face, separated into two triangle through `glDrawElements`.

We only draw faces which are "visible" by a very stupid algorithm: If there is a neighboring block covering this face, we don't draw it. This is stupid and won't eliminate all blocks, but the great majority, the world is mostly solid.

We can use a very specific information: The world won't change much. We keep a buffer of vertices for each chunk in memory and update it when the world changes. This doesn't seem to be overly inefficient since the update is seldom called for. A draw call is made for every chunk.

The updating of the chunk buffers is of course the expensive part. This can be implemented fairly efficient. Sending the data to the GPU every frame is not too bad since it is only required when the chunk updates and even if every chunk is update on the same time: The vertices hold very little data and there aren't many of them.

### Specification
A draw call is made for every chunk which holds a buffer of the faces (vertices + indices: `glDrawElements`).

- The vertex shader has access to the following uniforms:
  - Projection matrix
  - View matrix
- The vertex shader inputs consist out of (per vertex):
  - texture coordinate
  - position in world space
- The vertex shader computes `gl_Position` in NDC
- The vertex shader passes to the fragment shader:
  - The normal of a primitive
  - The position in camera space
- The fragment shader has access to:
  - The position of light sources in camera space




## UI
UI rendering is of course handled separately:
There need to be two ways to access coordinates: Absolutely in pixel size and relative in percent. With these two, is the thought, can be every thing expressed.
We need of course a whole new pipeline for UI rendering which isn't a big problem. With percent values, we have the great advantage that we can simply interpret them as coordinates in NDC so that any transformation is unnecessary. Transforming the absolute parts is very simple, we do it on the GPU so that everything is in one place (though it really doesn't matter).
We of course need to tell the GPU the size of the display so that it may transform accurately.

A new UIVertex class is necessary. Handling textures is the difficult part. We need to generate textures on the fly, especially for changing text.

### Specification

- The vertex shader has access to the following uniforms:
  - The size of the window in pixels as vec2
- The vertex shader inputs per vertex:
  - The texture coordinates (maybe as vec3?) because of texture array
  - The percent and absolute positioning
- The fragment shader receives from the vertex shader
  - the interpolated texure coordinates