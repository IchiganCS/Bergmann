# Rendering

This article describes how the rendering is done in this project.

## Thoughts

Drawing a whole block is bad. You can possibly only see three of all faces, drawing six is irresponsibly expensive. For most blocks, they won't be visible at all.
It seems that providing a unified way to render block*faces* is better overall and should be used. A disadvantage is the non-reusability of vertices. That's ok, using the same vertex while interpolating for different textures on the same block seems impossible to do right.

A face can be identified by the position of a block and its direction. It needs to have meaningful texture coordinates (which should also be usable with a texture atlas).

However, we can't use the geometry shader, since the output of the vertex shader is in NDC. Generating new primitives in NDC seems to be impossible.

So we now know: The vertex shader needs four inputs for each face, separated into two triangle through `glDrawElements`.

We only draw faces which are "visible" by a very stupid algorithm: If there is a neighboring block covering this face, we don't draw it. This is stupid and won't eliminate all blocks, but the great majority, the world is mostly solid.

We can use a very specific information: The world won't change much. We keep a buffer of vertices for each chunk in memory and update it when the world changes. This doesn't seem to be overly inefficient since the update is seldom called for. A draw call is made for every chunk.

The updating of the chunk buffers is of course the expensive part. This can be implemented fairly efficient. Sending the data to the GPU every frame is not too bad since it is only required when the chunk updates and even if every chunk is update on the same time: The vertices hold very little data and there aren't many of them.

## Specification

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
  - The position in world space
- The fragment shader has access to:
  - The position of light sources in world space
