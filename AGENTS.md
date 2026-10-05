# AGENTS.md

## Project goal

Remake the original XNA rally game for the web while preserving its spirit, handling, procedural track generation, race structure, and recognizable visual identity.

The remake should use modern web tooling and modern rendering/physics techniques. The old XNA implementation is a behavioral and visual reference, not an architecture to reproduce literally.

Networking is out of scope for the initial remake.

## Legacy code

The existing XNA projects are reference material.

- Do not restructure or modernize the legacy XNA code unless explicitly asked.
- Prefer reading the original implementation before recreating behavior.
- Preserve important gameplay constants and algorithms unless a deliberate change is documented.
- Do not port old rendering infrastructure line-for-line when a simpler modern equivalent exists.
- When behavior intentionally differs from the original, document why.

## New web implementation

Keep the remake isolated from the legacy projects under:

```text
web/
  src/
    app/
    game/
      camera/
      input/
      race/
      vehicle/
      world/
        terrain/
        track/
        scenery/
      rendering/
      audio/
      ui/
    core/
      math/
      random/
      timing/
    assets/
  public/
  tests/
```

This structure is a direction, not an excuse to create empty layers. Only add a folder or abstraction when there is real code that belongs there.

## Preferred stack

Unless there is a clear reason to change it:

- TypeScript
- Vite
- Three.js
- Rapier 3D
- Web Audio
- HTML/CSS for menus and HUD

Prefer browser-native APIs and small focused dependencies over large frameworks.

## Architecture rules

### Separate simulation from presentation

Gameplay state must not depend on Three.js objects.

For example, the vehicle simulation should own position, rotation, speed, steering and suspension state. Rendering should read that state and update the Three.js scene.

This separation should make it possible to test simulation without a renderer.

### Fixed-step gameplay

Run gameplay simulation at a fixed 60 Hz timestep.

Rendering may run at the display refresh rate and interpolate between simulation states.

This is especially important because the original car handling was effectively frame-step based.

### Keep procedural generation deterministic

Track and terrain generation must be reproducible from a seed.

If matching the original C# seed output is implemented, use an explicitly named compatibility random generator rather than relying on `Math.random()`.

Never use implicit global randomness in procedural generation.

### Physics is a tool, not the game design

Rapier should improve ground contact, suspension, collisions, props and vehicle interactions.

Do not replace the character of the original handling merely because a more physically realistic simulation is possible.

Tune the modern vehicle against the original game as a reference.

### Rendering should reproduce intent, not old implementation details

Use modern Three.js facilities for lighting, shadows, materials, reflections, particles and post-processing.

Custom shaders/materials are appropriate where they define the look, especially terrain blending.

Do not recreate obsolete XNA render-target or shadow-map architecture just for implementation parity.

## Code quality

Readability is a primary requirement.

- Prefer straightforward code over clever code.
- Use descriptive names. Avoid unexplained abbreviations.
- Keep functions focused and reasonably small.
- Prefer early returns over deeply nested conditionals.
- Keep side effects obvious.
- Avoid hidden global state.
- Avoid inheritance-heavy designs unless inheritance genuinely models the domain.
- Prefer composition and plain data structures.
- Do not add generic abstractions for a single use case.
- Remove dead code rather than commenting it out.
- Do not leave large TODO blocks without an issue or clear next action.

Comments should explain intent, constraints, compatibility behavior, or non-obvious mathematics. Do not comment code that is already self-explanatory.

## TypeScript rules

- Keep strict TypeScript enabled.
- Avoid `any`.
- Prefer explicit domain types for important concepts such as seeds, race state and vehicle state.
- Keep Three.js and Rapier types at subsystem boundaries where possible rather than spreading them through gameplay code.
- Prefer immutable inputs for generation functions when practical.
- Use constants with names instead of unexplained numeric literals.

When porting an original formula, keep the original constant values together and document their source.

Example:

```ts
// Original XNA handling values from Car.cs / CarControlComponent.cs.
// Keep these together so the baseline handling can be compared easily.
export const ORIGINAL_HANDLING = {
  acceleration: 0.15,
  deceleration: 0.35,
  maxSpeed: 50,
  friction: 0.995,
} as const;
```

## File organization

Each file should have one clear responsibility.

Avoid large manager classes that own unrelated systems.

Prefer:

```text
vehicle/
  vehicleState.ts
  vehicleController.ts
  vehiclePhysics.ts
  vehicleRenderer.ts
```

over a single `VehicleManager.ts` containing input, simulation, rendering, sound and UI.

Similarly, procedural generation should be split by concept:

```text
world/
  track/
    trackCurve.ts
    trackRasterization.ts
  terrain/
    heightMap.ts
    terrainGeometry.ts
    terrainMaterial.ts
```

Do not split tiny cohesive code purely to satisfy a folder pattern.

## Naming

Use names from the game domain where possible.

Good:

- `TrackCurve`
- `HeightMap`
- `RaceState`
- `VehicleController`
- `CheckpointTracker`

Avoid vague names such as:

- `Manager`
- `Helper`
- `Utils`
- `Data`

unless the name genuinely describes the responsibility.

## Testing priorities

Automated tests are especially valuable for deterministic systems.

Prioritize tests for:

1. seeded random compatibility
2. track generation
3. height-map generation
4. curve/rasterization math
5. checkpoint and lap progression
6. fixed-step vehicle behavior

Rendering tests may use visual/manual comparison where unit tests are not useful.

## Performance

Do not optimize blindly.

Measure before adding complexity.

Likely performance-sensitive areas include:

- generated terrain geometry
- foliage/object counts
- particles
- shadow rendering
- post-processing
- physics collider complexity

Prefer instancing, shared geometry/materials and sensible collider simplification where appropriate.

## Initial implementation order

Work toward a playable baseline before visual polish.

1. Bootstrap the TypeScript/Three.js application.
2. Implement deterministic random generation.
3. Port track-curve generation.
4. Port height-map and road generation.
5. Build the terrain mesh.
6. Add the original car model.
7. Reproduce the original fixed-step handling as a baseline.
8. Add the chase camera.
9. Add checkpoint/lap logic.
10. Introduce Rapier and tune a raycast vehicle against the baseline.
11. Rebuild terrain materials, lighting and atmosphere.
12. Add particles, weather, post-processing, audio and UI.

Do not start by porting menus, networking or every legacy effect.

## Decision standard

When choosing between preserving an old implementation and using a modern approach, ask:

> Does this affect the identity or feel of the game, or was it merely how the 2013 XNA version had to achieve the result?

Preserve the former. Modernize the latter.
