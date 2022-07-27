# World

This covers chunks (their generation too) and some other topics.

## Coordinates
We use a left handed coordinate system. Positive directions each: x to the right, y to the top, z to the back.
There are a few different spaces.

- Block space: Somtimes referred to as model space; Relative to the origin of the block which always is at the lower left front point.
- Chunk space: Relative to the origin of the chunk which always is at the lower left front point.
- World chunk space: Relative to the chunk origin of the chunk at (0, 0, 0). It is scaled down so that a movement of 1 is actually one chunk.
- World space: World chunk space but without the scaling.
- Camera space (only used in the shaders): The camera is set as the origin and has no rotation. All other elements are transformed accordingly.

The `Block` class defines many static helper functions, you can take a look at those.

## Chunks
Each chunk has `CHUNK_SIZE` in dimensions which is 16. This number will not change. How should chunks be organized. For x and z coordinates, this works fine.
However, let's assume we don't treat chunks as an entire column but only as one block of 16 * 16 * 16. This has better loading time and in some cases is more efficient.
But we if we only load a sphere of chunks instead of an rectangle of chunk columns, we could potentially end up with being able to look into the world.
Assume there is a mountain far away. We're on height 70 and the mountain reaches up to 200. Potentially, we only render a part of that chunk column and if we move closer, we eventually end up seeing the entire mountain when the upper chunks load. 

Our solution: none. Setting the render distance high enough will eventually solve the problem.

This enables us to work without a height restriction. This doesn't seem especially useful since a fixed lowest height for mining is good. Let's set it 0.
