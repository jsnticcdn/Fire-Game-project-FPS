# Advanced FPS Movement System

**Version:** 1.1  
**Author:** AZE  
**Unity Version:** 6000.0+ (Unity 6)

---

## Overview

The **Advanced First Person Movement System** is a scalable character controller built on a **Finite State Machine (FSM)** architecture. It provides a fluid First-Person experience with built-in physics interactions, procedural camera animations ("game juice"), and a modular codebase designed for easy extension.

### Key Features
* **FSM Architecture:** Clean state separation (Idle, Walk, Sprint, Jump, Fall, Crouch, Slide, Dodge, Slope Slide).
* **Crouch Slide:** Sprinting into a crouch becomes a momentum-preserving slide that accelerates downhill, chains into slide-jumps, and triggers on fast landings with crouch held.
* **Advanced Jump Mechanics:** Includes Coyote Time, Jump Buffering and crouch-jumping for precise and responsive platforming.
* **Slope Handling:** Movement is projected onto the ground plane, steep surfaces make the player slide, and the controller stays glued to the floor when running downhill.
* **Edge Handling:** Sphere-cast ground probing instead of `CharacterController.isGrounded`, no ledge snagging while airborne, and upward velocity is cancelled on ceiling hits.
* **Physics-Based:** Custom gravity and ground detection.
* **Framerate-Independent Smoothing:** All interpolation uses exponential damping, so movement feels the same at 30, 60 or 240 FPS.
* **Events API:** `StateChanged` and `Landed` events plus a public `CurrentState`, so footsteps, animation, audio and fall damage hook in without touching the asset's code.
* **Procedural Camera Effects:** Dynamic Head Bob, Tilt, and FOV kicks powered by Cinemachine.
* **Moving Platforms:** A waypoint-driven platform component, and a player that is carried, in translation and rotation, by any moving surface it stands on and inherits its momentum when leaving it.
* **Interaction System:** Physics-based pushing of dynamic objects, with optional mass-based scaling.
* **New Input System:** Fully integrated with Unity's `com.unity.inputsystem`, including toggle or hold crouch and gamepad-friendly double-tap dodge.

---

## 1. Dependencies (IMPORTANT)

Go to **Window > Package Manager > Unity Registry** and install:

1.  **Input System** (`com.unity.inputsystem`): **required**.
    * *Critical Step:* Go to **Project Settings > Player > Active Input Handling** and set it to **"Input System Package (New)"** or **"Both"**.
2.  **Cinemachine 3.x** (`com.unity.cinemachine`): **required for the camera effects and the demo prefab**.
    * Version 3 is required. The camera scripts use the `Unity.Cinemachine` namespace and the `CinemachineCamera` component, which do not exist in Cinemachine 2.x.
3.  **Universal Render Pipeline** (`com.unity.render-pipelines.universal`): **required for the demo visuals**.
    * The demo materials use the URP Lit shader and the demo scene uses a URP Volume profile. The movement scripts themselves are render-pipeline-independent; in a Built-in RP or HDRP project everything still works, but the demo materials must be replaced.

> **Note:** The asset ships with an assembly definition that detects Cinemachine automatically. If Cinemachine is missing the project still compiles and the player still moves; only `CameraEffects` is excluded from compilation (the prefab will show it as a missing script until the package is installed).

---

## 2. Quick Setup

1.  **Import the Package:** Ensure all files are imported into your project folder.
2.  **Setup Scene:**
    * Create a new scene or open an existing one.
    * Ensure there is a floor with a **Collider** for the player to walk on.
3.  **Add the Player:**
    * Navigate to `Assets/StudiosAZE/Advanced First Person Movement System/Prefabs/`.
    * Drag and drop the **Player** prefab into your scene.
4.  **Configure Inputs:**
    * Select the `Player` object in the Hierarchy.
    * Locate the `PlayerInputHandler` component in the Inspector.
    * The **Input Action References** (Move, Look, Jump, Sprint, Crouch, Dodge) come pre-assigned to the bundled action asset.
    * *To use your own actions:* Drag them from your `.inputactions` asset into the respective slots. If any reference is left empty, the player logs a descriptive error on load instead of throwing exceptions.

---

## 3. Architecture Overview

This asset uses a **Finite State Machine (FSM)** to handle movement logic. Exactly one state is active at a time, and states never run movement code for a frame in which they were replaced.

### The Frame Loop

`PlayerMovementStateMachine.Update()` runs the same steps every frame:

1.  **Carry** the player by whatever the surface underneath moved, then sync the physics scene.
2.  **Probe the ground**, refreshing `IsGrounded`, `GroundNormal` and `GroundAngle`, and fire `Landed` if the player just touched down.
3.  Update the Coyote Time and Jump Buffer counters.
4.  **Resolve transitions:** repeatedly call `TryTransition()` on the active state until it returns `false` (capped at 4 switches per frame).
5.  Apply gravity, then ask the surviving state for its **`MovementIntent`** and blend the current velocity toward it.
6.  Apply the accumulated velocity through the `CharacterController`.
7.  **Cancel upward velocity** if the capsule hit a ceiling.
8.  **Snap to the ground** so downhill movement never drifts off the surface.
9.  Interpolate the capsule height and camera position (crouch).

Because transitions are resolved *before* the intent is built, a single frame can never run two states' movement logic, and a state can never act after it has been exited.

### Core Scripts

* **`PlayerMovementStateMachine.cs` (Context):** The central controller. Owns references, tunables, shared runtime data (velocity, counters), the frame ordering, the events, and the movement helpers every state calls.
* **`GroundSensor.cs`:** A plain C# class owned by the context that does all ground physics: the sphere-cast probe, normal refinement, surface tracking for moving platforms, and the snap measurement. The context's `IsGrounded` / `GroundNormal` / `GroundAngle` / `OnSteepGround` read through to it, so states never touch it directly.
* **`CapsuleHeightController.cs`:** A plain C# class owned by the context that handles the capsule: standing/crouch heights, the ceiling check for standing up, and the height + camera interpolation. The capsule never grows without clearance: any state may request standing height, and the interpolation simply holds the crouch until there is headroom.
* **`PlayerStateFactory.cs`:** Instantiates every state once and exposes them as properties. Also provides `ResolveGroundedState()`, the single place that decides which state the player belongs in once they are on the ground.
* **`PlayerBaseState.cs`:** Abstract base class defining the contract shared by all states.
* **`MovementIntent.cs`:** The value a state returns each frame to describe how it wants to move.
* **`MotionMath.cs`:** The exponential damping used by every interpolation in the asset.

### The State Contract

`PlayerBaseState` supplies working defaults so a state only overrides what makes it different:

| Member | Kind | Default behaviour |
|---|---|---|
| `MoveSpeed` | `protected virtual float` | `ctx.WalkSpeed` |
| `Enter()` | `public virtual void` | Sets `ctx.TargetHeight` to standing height |
| `BuildIntent()` | `public virtual MovementIntent` | Grounded movement at `MoveSpeed`, smoothed by `Movement Smoothing` |
| `Exit()` | `public virtual void` | Nothing |
| `TryTransition()` | `public abstract bool` | Must be implemented |
| `TryLeaveGround()` | `protected bool` | Hands off to Fall or Slope Slide when the ground stops supporting the state |
| `SwitchTo(state)` | `protected bool` | Switches state and returns `true` |

`TryTransition()` returns `true` when it changed state and `false` otherwise. Writing every branch as `return SwitchTo(...)` makes it impossible to accidentally run two transitions in one call.

### Movement Intents

States never write velocity directly. Once per frame the active state returns a `MovementIntent`, and the context blends the player's velocity toward it:

* **`MovementIntent.Smoothed(targetVelocity, smoothing)`**: the velocity is exponentially damped toward the target. This is what locomotion, air and slide states use.
* **`MovementIntent.Immediate(targetVelocity)`**: the velocity is replaced outright, bypassing smoothing. This is what scripted motion like the dodge uses.

Because the state only *describes* motion and the context *applies* it, a state can never leave a half-applied velocity behind when it exits, and all smoothing lives in one place: `MotionMath.DampFactor`, which converts a smoothing rate into a framerate-independent interpolation factor.

### State Families

* **`PlayerLocomotionState`**: the shared, upright, grounded behaviour. It owns the transition ladder (fall > slope slide > jump > slide > crouch > dodge > speed change) used by **Idle**, **Walk** and **Sprint**, which therefore only declare their `MoveSpeed`.
* **`PlayerAirborneState`**: the shared airborne behaviour used by **Jump** and **Fall**: moves at `AirSpeed` with `Air Smoothing` and the `Air Control` blend, and keeps the capsule crouched while crouch is held.
* **Standalone states**: **Crouch**, **Slide**, **Dodge** and **Slope Slide** derive directly from `PlayerBaseState` because each has its own exit conditions.

### Available States

* **Idle:** Grounded, no input. Decelerates to a stop using `Movement Smoothing`.
* **Walk:** Standard grounded movement.
* **Sprint:** Accelerated grounded movement. Optionally restricted to forward input.
* **Jump:** Applies instantaneous vertical force, then hands off to Fall once falling.
* **Fall:** Falling physics and air control. Consumes a buffered jump while Coyote Time is active.
* **Crouch:** Reduced capsule height and speed. Refuses to stand up under a ceiling, and jumps straight out of the crouch when there is headroom.
* **Slide:** Entered by crouching above `Slide Min Speed`: while sprinting, or on landing with crouch held. Starts at the player's current velocity and decays with friction; slopes accelerate or brake it depending on steepness and how aligned the slide is with the fall line. Slows into Crouch, stands up when crouch is released (headroom permitting), and can jump out with full momentum.
* **Dodge:** Timed directional dash with a cooldown, triggered by double-tapping a direction.
* **Slope Slide:** Entered automatically on surfaces steeper than the `CharacterController`'s **Slope Limit**. Slides down the fall line with reduced steering. Jumping is disabled here, so the slope cannot be climbed by jumping repeatedly.

### Events & Public State

The state machine exposes everything an external system needs to react to movement without modifying the asset:

* **`CurrentState`**: the active state instance. Compare it against the instances on `States` (the factory): `machine.CurrentState == machine.States.Sprint`.
* **`StateChanged`**: `Action<PlayerBaseState, PlayerBaseState>`, fired after every transition with `(previousState, newState)`.
* **`Landed`**: `Action<float>`, fired on the frame the player touches ground after being airborne. The parameter is the vertical velocity at impact (negative), which is exactly what fall damage, landing audio and camera shake need. The value is consumed by the gravity reset immediately afterwards, so this event is the only place to read it.

```csharp
public class FallDamage : MonoBehaviour
{
    [SerializeField] private PlayerMovementStateMachine movement;

    private void OnEnable() => movement.Landed += HandleLanded;
    private void OnDisable() => movement.Landed -= HandleLanded;

    private void HandleLanded(float impactVelocity)
    {
        if (impactVelocity < -12f)
        {
            Debug.Log($"Hard landing at {impactVelocity:F1} m/s");
        }
    }
}
```

### Context Helpers

States are expected to ask the context questions rather than reading raw input:

* `HasMoveInput`, `SprintHeld`: movement intent, deadzone already applied.
* `JumpRequested`, `CrouchRequested`: respect both the input and the `Use Jump` / `Use Crouch` toggles.
* `TryConsumeDodge()`: returns `true` only if a dodge is allowed right now. A blocked dodge stays buffered until it expires (0.2s), so it can still fire the moment its conditions are met.
* `CanStandUp()`: ceiling check for leaving the crouch state.
* `IsGrounded`, `GroundNormal`, `GroundAngle`: result of the ground probe.
* `OnSteepGround`: standing on ground steeper than the **Slope Limit**. Purely geometric, and the guard used by everything that must not work on an unwalkable surface.
* `ShouldSlopeSlide`: whether the Slope Slide state applies (`OnSteepGround` plus the `Use Slope Slide` toggle).
* `SlideRequested`: whether a crouch press should become a slide instead (`Use Slide`, crouch input, grounded, and moving above `Slide Min Speed`).
* `GetInputWorldVector()`, `BuildGroundedVelocity(speed)`, `BuildAirborneVelocity(speed)`: the movement primitives. A state that needs its own motion builds a target velocity with these and returns it inside a `MovementIntent`.
* `ConstrainVelocityToGround()`: projects the current velocity onto the ground plane. Slope Slide runs it every frame so arriving velocity can never launch the capsule off the surface.
* `StandingHeight`, `CrouchHeight`, `TargetHeight`: capsule height plumbing.
* `InputHandler.CancelCrouch()`: clears the crouch toggle, used when a mechanic (like the crouch-jump) overrides the player's crouch intent.

### Ground, Slopes and Edges

`CharacterController` solves very little on its own, so `GroundSensor` layers the following on top of it, with the context deciding when each piece runs.

**Ground probing.** `CharacterController.isGrounded` is only true when the *last* `Move()` produced a downward collision, so it flickers on slopes, stairs and ledges. It is not used. A sphere is cast down from the bottom of the capsule each frame instead, giving grounded state, surface normal and slope angle in one query. It ignores the player's own collider and any surface steeper than a wall, so brushing against geometry never registers as ground. The sphere is nearly as wide as the capsule, so standing half-off a ledge still counts as grounded.

**Reading the slope angle.** That width is also why the sphere cannot supply the normal: it touches the nose of a step before the player reaches it, and on a convex edge it returns a value interpolated between the two faces: a flat stair tread reads as a steep slope. The angle therefore comes from a separate raycast straight down the **capsule axis**, which lands on the surface actually being stood on. If that ray misses (perched on a narrow ledge over a drop) it falls back to a ray at the sphere contact point, then to the sphere normal. `GroundAngle` gates **Step Offset**, so a misread here stalls the player against stairs.

**Walking on slopes.** Movement direction is projected onto the ground plane (`Vector3.ProjectOnPlane`). Without it the player moves horizontally into a ramp and depends on **Step Offset** to climb, which is what produces the familiar stuttering. Projection also keeps speed constant along the surface instead of dropping as the slope steepens.

**Running downhill.** Projection alone still lets the player leave the surface on convex breaks, bouncing down long ramps. After the move is applied, `SnapToGround()` casts again and pulls the capsule back to the surface if it is within **Ground Snap Distance**. Snapping is skipped while jumping, on unwalkable ground, and while the surface underfoot is itself moving: a ridden platform already keeps the player seated, and the snap exists for terrain, not vehicles.

The gap it corrects is measured against the controller's *rest* separation, not against zero: a settled `CharacterController` holds itself about `skinWidth` off the surface, and that separation grows with `1 / cos(angle)` on slopes. Comparing against zero makes the snap fire on ground the player is already standing on, pushing the capsule into the floor for physics to push back out every frame. The formulation is deliberately conservative: if the controller ever rests closer than `skinWidth`, the measured gap goes negative and the snap simply stays quiet, so it can never inject a downward push at rest.

**Steep slopes.** Anything above the `CharacterController`'s **Slope Limit** is unwalkable. Vanilla `CharacterController` refuses to move up it and leaves the player standing on the cliff face; the FSM switches to **Slope Slide** instead, driving them down the fall line at a rate scaled by steepness with a fraction of steering left.

While sliding, the player's velocity is constrained to the surface plane every frame. Landing on the face at speed (or running onto it) arrives with mostly *horizontal* velocity, which points off the slope; smoothing alone takes a few frames to bend it downhill, and the ground snap is deliberately disabled on steep ground, so nothing would restore contact. Left unconstrained, that excess sends the capsule skipping down the face in small hops. Projecting the velocity onto the plane converts it into slide motion instantly, so contact never breaks.

Blocking the *walk* is not enough, because a jump bypasses it: the player rises, lands higher up the face and jumps again. `OnSteepGround` therefore gates jumping, Coyote Time, dodging and **Step Offset**, and skips the ground snap; any one of them left open is enough to climb a cliff. It is deliberately independent of the `Use Slope Slide` toggle: turning the slide off restores the vanilla behaviour but must not hand back a way to climb. Gravity still pulls the player down the surface, so the player is never stuck.

**Edges.** **Step Offset** is set to zero while airborne and restored on landing. Left enabled, it lets a falling capsule snap up onto ledges it merely grazed. Upward velocity is also cancelled when `CollisionFlags.Above` reports a ceiling hit, so jumping into a low roof no longer keeps the player pinned against it for the rest of the ascent.

**Moving platforms.** `CharacterController` inherits velocity from nothing: on a platform that moves sideways it is left behind, and on one that rises it gets shoved through. The ground probe already identifies the collider underneath, so the controller remembers that transform and, at the very start of its own `Update`, shifts itself by however far the point it is standing on moved since last frame, including movement produced by the surface rotating, and turns with the surface's yaw so a spinning platform carries the player around instead of sliding out from under them.

The carry is a direct transform shift, **not** a `Move()` call, and that distinction is what makes it smooth. Standing on a platform means the player's frame of reference moved, so the carry must restore the pose *relative to the surface* exactly. Running it through `Move()` sweeps and collides: on a rising platform the surface has already entered the capsule by the frame's delta, so a swept carry resolves through PhysX's penetration recovery every frame, which is visible as jitter and can overshoot at speed. The direct shift reproduces last frame's settled pose by construction, at any platform speed, and gravity then re-seats the capsule with an ordinary collision-checked `Move()`. The rare crush case (a platform pushing the player into other geometry) is resolved by that same move's penetration recovery. Because the carry happens before the ground probe, `IsGrounded` and the normal are always measured from the settled pose: grounding does not flicker no matter how fast the platform travels. A side effect of bypassing `Move()` is that `Controller.velocity` ignores the ride, so head bob and FOV kick correctly stay idle on a moving elevator.

This is deliberately not tied to `MovingPlatform`: any surface works: animated, tweened, or driven by a kinematic Rigidbody. Static geometry produces a zero delta and costs nothing. `MovingPlatform` carries `[DefaultExecutionOrder(-100)]` so it has already moved by the time the controller samples it; a platform driven by something else that moves in `LateUpdate` will lag the player by one frame.

**Why `Physics.SyncTransforms()` runs right after the carry.** `Physics.autoSyncTransforms` has defaulted to `false` since Unity 2018, so writing `transform.position` (the platform moving itself, or the carry moving the player) does not immediately update the physics scene. Without an explicit sync, sphere-casts and `Move()` collide against stale collider poses: the player is carried by one figure while colliding against another. That mismatch is invisible on static geometry, which never goes stale, and shows up as jitter the moment a surface moves. One call placed after the carry covers everything written this frame: every platform's move and the player's own shift.

For the same reason `MovingPlatform` requires a `Rigidbody` and forces it kinematic on `Awake`. A collider with no Rigidbody is a *static* collider, and moving one makes PhysX rebuild its static broadphase every frame.

> **Momentum is inherited.** The velocity of the surface underfoot is measured every frame, and the moment the player leaves the ground (jumping or walking off), it is added to their own, so a fast platform never slides out from under a jump. Upward velocity is inherited too; downward is not, so a descending elevator cannot weaken a jump.

> **Dynamic Rigidbodies are not carried.** The carry exists for the player. The platform moves by transform, which produces no friction velocity in PhysX, so a loose box on top slides rather than riding; parent it to the platform if it must travel along.

> **`Min Move Distance`:** the `CharacterController` field is forced to `0` on `Awake`. Unity's default of `0.001` silently discards any move smaller than a millimetre, which breaks ground snapping and platform riding, and causes stalls at low speed.

### Camera & Interaction

* **`PlayerCameraController.cs`:** Handles input look (Mouse/Gamepad), clamping, sensitivity, Y inversion, and cursor locking (`SetCursorLocked`).
* **`CameraEffects.cs`:** Adds polish (Head Bob, Dutch Tilt, FOV Kick). Compiled only when Cinemachine 3.x is installed.
* **`CharacterPushInteraction.cs`:** Allows the player to push Rigidbody objects, optionally scaled by mass.
* **`MovingPlatform.cs`:** Waypoint-driven platform. Drop it on any object with a Collider; the player needs no setup to ride it.

---

## 4. Configuration & Tuning

Settings are organized by component in the Inspector.

### Player Movement State Machine
* **General Toggles:**
    * `Use Jump`: Enable/Disable jumping.
    * `Use Crouch`: Enable/Disable crouching.
    * `Use Slide`: Enable/Disable the crouch slide.
    * `Use Dodge`: Enable/Disable the dash ability.
* **Speed Settings:**
    * `Walk Speed` / `Run Speed` / `Crouch Speed`: Movement velocities.
    * `Movement Smoothing`: Interpolation rate for acceleration/deceleration. Framerate-independent.
    * `Sprint Forward Only`: When enabled, sprinting requires forward input: no backwards or purely lateral sprinting.
* **Physics:**
    * `Gravity`: Custom gravity force (Default: -15).
    * `Initial Fall Velocity`: The small downward velocity applied while grounded (Default: -3). It keeps the capsule pressed to the floor; more negative values stick harder to abrupt geometry changes.
* **Ground Detection:**
    * `Ground Layers`: Which layers count as walkable ground. The player's own collider is always ignored.
    * `Ground Check Distance`: How far below the capsule the ground probe reaches before the player is considered airborne.
    * `Ground Snap Distance`: How far the capsule may be pulled back down to keep contact when running downhill. Raise it for steeper terrain; set it to `0` to disable snapping.
* **Slope Settings:**
    * `Use Slope Slide`: Enable/Disable sliding on unwalkable slopes. Disabling it does not make steep slopes climbable; jumping and dodging stay blocked either way.
    * `Slope Slide Speed`: Downhill speed on a vertical surface. Shallower slopes slide proportionally slower.
    * `Slope Slide Control`: How much steering the player keeps while sliding, as a fraction of `Walk Speed`. `0` removes control entirely.

> The threshold between *walkable* and *slide* is the **Slope Limit** on the `CharacterController` component itself (default 45°), not a separate setting.
* **Jump Settings:** 
    * `Jump Force`: Upward velocity applied on jump.
    * `Coyote Time`: The duration (in seconds) the player can still jump after walking off a ledge.
    * `Jump Buffer Time`: The duration (in seconds) a jump input will be stored before hitting the ground.
* **Air Control:**
    * `Air Control`: How much input redirects momentum while airborne. `1` gives full steering (default), `0` locks the launch trajectory entirely.
    * `Jump Speed Retention`: How much horizontal speed each consecutive jump keeps.
    * `Air Smoothing`: Interpolation rate for airborne direction changes.
* **Dodge Settings:**
    * `Dodge Speed`: Velocity during dash.
    * `Dodge Duration`: Duration of the dash force.
    * `Dodge Cooldown`: Time required between dashes.
* **Crouch Settings:**
    * `Crouch Transition Speed`: Smoothness of height change.
    * `Crouch Height Ratio`: Crouched capsule height as a fraction of standing height (Default: 0.5).
    * `Camera Offset`: Camera height adjustment when crouching.
    * `Ceiling Layers`: Which layers can block the player from standing up (Default: everything).
* **Slide Settings:**
    * `Slide Min Speed`: Minimum horizontal speed for a crouch press to become a slide. Set it between `Walk Speed` and `Run Speed`.
    * `Slide Friction`: How fast the slide decays on flat ground, in m/s².
    * `Slide Slope Acceleration`: How strongly slopes act on the slide. The effect scales with steepness and direction: sliding down a slope accelerates, sliding up brakes hard.
    * `Slide Max Speed`: Speed cap on long downhill slides.
    * `Slide Steering`: How fast the slide direction can be steered, in radians per second. `0` locks the launch direction.

### Player Input Handler
* **Input Action References:** Move, Look, Jump, Sprint, Crouch, Dodge.
* **Crouch Mode:** `Toggle` (press to crouch, press to stand) or `Hold` (crouch only while the button is held).

### Camera Effects
* **Head Bob:**
    * `Use Head Bob`: Toggle on/off.
    * *Settings:* Adjust Amplitude, Frequency, and Smoothing.
* **Camera Tilt (Dutch):**
    * `Use Tilt`: Toggle on/off.
    * *Settings:* Max Tilt Angle (e.g., 1.5°) and Smoothing.
* **Slide Tilt:**
    * `Use Slide Tilt`: Toggle on/off.
    * *Settings:* Slide Tilt Angle, a fixed Dutch tilt applied while sliding. Head bob is automatically suppressed during a slide so the glide stays smooth.
* **Dynamic FOV:**
    * `Use FOV Kick`: Toggle on/off (sprinting effect).
    * *Settings:* FOV Boost Amount and Smoothing.

### Player Camera Controller
* **Sensitivity:** Controls look speed (X and Y axis). `Invert Y` flips the vertical axis.
* **Max Look Up / Down Angle:** Vertical look limits in degrees (Default: 90 up, 75 down).
* **Lock Cursor On Start:** Locks and hides the cursor on load. Call `SetCursorLocked(bool)` from your own code to release it for pause menus and UI.

### Physics Interaction (Push)
* **Push Power:** Base strength of the push.
* **Weight Based Push:** Reference mass (in kg) for the push. Objects at or below this mass receive the full push; heavier objects are pushed proportionally less. Set to `0` to push everything at full strength regardless of mass. Pushed objects keep their vertical velocity, so falling objects keep falling.

### Moving Platform

Add `MovingPlatform` to any object with a Collider. A kinematic `Rigidbody` is added and configured automatically, and the player requires no setup.

* **Waypoints:** Offsets from the platform's starting position, not world coordinates: move the platform in the Scene and the whole path follows it. The starting position is already the first point, so a simple elevator is a single waypoint of `(0, 3, 0)`. Any direction works, including diagonals.
* **Mode:** `PingPong` retraces the path, `Loop` closes the circuit back to the start, `Once` stops at the last point.
* **Speed:** Units per second. With `Use Easing` on this becomes the *average* speed, since each leg accelerates and decelerates.
* **Wait Time:** Pause on arrival at every point.
* **Auto Start:** Turn off to drive it from script with `StartMoving()` / `StopMoving()` / `ResetToStart()`.

Select the platform in the Scene view to see its path drawn as a gizmo. `Velocity` is exposed for effects, audio, or gameplay that needs to know how fast the platform is travelling.

---

## 5. Extending the System (For Programmers)

The asset ships with an assembly definition, **`AZE.AdvancedFirstPerson`**. It is auto-referenced, so scripts in your project's default assembly (`Assembly-CSharp`) can use every class with no setup. If your code lives in its own assembly definition, add `AZE.AdvancedFirstPerson` to its references.

To add a new mechanic (e.g., a shoulder "Tackle"), follow this workflow.

### Step 1: Add the Input

1.  Open `StudiosAZE/Advanced First Person Movement System/Inputs/InputSystem_Actions_Movement`.
2.  Add a new Action (e.g., "Tackle") and assign a binding.
3.  In `PlayerInputHandler.cs`:
    ```csharp
    [SerializeField] private InputActionReference tackleAction;

    public bool TackleTriggered => tackleAction.action.WasPressedThisFrame();

    // In OnEnable():
    tackleAction.action.Enable();

    // In OnDisable():
    tackleAction.action.Disable();
    ```

### Step 2: Create the State

Create `PlayerTackleState.cs` inheriting from `PlayerBaseState`. Override only what differs from the defaults:

```csharp
public class PlayerTackleState : PlayerBaseState
{
    private float _elapsed;

    public PlayerTackleState(PlayerMovementStateMachine ctx, PlayerStateFactory factory) : base(ctx, factory) { }

    protected override float MoveSpeed => ctx.RunSpeed;

    public override void Enter()
    {
        base.Enter();
        _elapsed = 0f;
    }

    public override MovementIntent BuildIntent()
    {
        _elapsed += Time.deltaTime;

        return MovementIntent.Smoothed(ctx.BuildGroundedVelocity(MoveSpeed), ctx.MovementSmoothing);
    }

    public override bool TryTransition()
    {
        if (TryLeaveGround()) return true;
        if (_elapsed < 0.5f) return false;

        return SwitchTo(factory.ResolveGroundedState());
    }
}
```

`BuildIntent()` is called exactly once per frame on the active state, after transitions are resolved, which makes it the safe place for per-frame timekeeping: the built-in Dodge state tracks its duration the same way, and the built-in Slide state updates its speed there.

Register it in `PlayerStateFactory.cs`:

```csharp
public PlayerBaseState Tackle { get; }

// In the constructor:
Tackle = new PlayerTackleState(_context, this);
```

Two rules make a grounded state behave correctly, and both are one call:

* **Start `TryTransition()` with `if (TryLeaveGround()) return true;`.** It hands off to Fall when the player walks off an edge and to Slope Slide when they end up on an unwalkable surface. Skipping it is how a custom state becomes a way to stand on a cliff face.
* **End with `factory.ResolveGroundedState()`** whenever the state finishes and hands control back to the ground. It is the only resolver, and it always returns a state that is legal for the surface the player is standing on.

### Step 3: Connect the Transition

Add the entry condition to the state family that should be able to start a tackle. Putting it in `PlayerLocomotionState.TryTransition()` makes it reachable from Idle, Walk and Sprint at once:

```csharp
public override bool TryTransition()
{
    if (TryLeaveGround()) return true;
    if (ctx.JumpRequested) return SwitchTo(factory.Jump);
    if (ctx.InputHandler.TackleTriggered) return SwitchTo(factory.Tackle);
    ...
}
```

Order matters: the first branch that returns `SwitchTo(...)` wins.

### Reacting Without New States

Not everything needs a state. Footsteps, animation parameters, landing audio and fall damage should subscribe to `StateChanged` and `Landed` from their own components instead; see **Events & Public State** above.

---

## 6. Upgrading from 1.0

Version 1.1 restructures the state machine. Existing scenes, prefabs and Inspector values are unaffected, but **custom states written against 1.0 must be updated**.

### Renamed members

| 1.0 | 1.1 |
|---|---|
| `EnterState()` | `Enter()` |
| `UpdateState()` | `BuildIntent()`, now returns a `MovementIntent` |
| `ExitState()` | `Exit()` |
| `CheckSwitchStates()` | `TryTransition()`, now returns `bool` |
| `InitializeSubState()` | Removed |
| `ctx.GetStandingHeight()` | `ctx.StandingHeight` |
| `ctx.GetCrouchHeight()` | `ctx.CrouchHeight` |
| `ctx.CanDodge()` | `ctx.TryConsumeDodge()` |
| `PlayerAirState.cs` / `factory.Air` | `PlayerFallState.cs` / `factory.Fall` |
| `PlayerBaseMovement.cs` | `PlayerMovementStateMachine.cs` (file renamed to match its class) |
| `PlayerInteraction.cs` | `CharacterPushInteraction.cs` (file renamed to match its class) |

### Behaviour changes

* **States no longer move the player directly.** A state returns a `MovementIntent` from `BuildIntent()` and the context applies it. Custom states that wrote `CurrentMoveVelocity` inside their update must return an intent instead.
* **Transitions no longer run inside the state's update.** Remove any `CheckSwitchStates()` call from your update logic; the state machine drives transitions itself.
* **Every transition branch must `return`.** Write `return SwitchTo(factory.X);` instead of a bare `SwitchState(...)` call. In 1.0 a state could enter and exit several states in one frame.
* **All smoothing is framerate-independent** (exponential damping). The same Inspector values now produce the same feel at any framerate; extremely high smoothing values ease slightly more softly than before.
* **`StateChanged` and `Landed` events were added**, along with public `CurrentState` and `States` properties.
* **A built-in crouch slide was added.** Crouching while moving faster than `Slide Min Speed` now enters **Slide** instead of Crouch, and landing at speed with crouch held slides too; set `Use Slide` to false to restore the 1.0 behaviour.
* **Crouch can jump.** Pressing jump while crouched stands up and jumps when there is headroom, consuming the buffered input.
* **Crouch supports Toggle and Hold modes** (`PlayerInputHandler > Crouch Mode`).
* **The dodge double-tap works on analog sticks.** Direction taps are compared by angle instead of exact value, and a buffered dodge expires after 0.2s.
* **Moving platforms carry rotation and their momentum is inherited** the moment the player leaves the ground, so jumping off a fast platform keeps its velocity.
* **Crouch is preserved while airborne and the capsule never grows without headroom**, so falling through a low passage cannot wedge the player against the ceiling.
* **Cursor locking is configurable** (`Lock Cursor On Start`, `SetCursorLocked(bool)`), and the vertical look axis can be inverted (`Invert Y`).
* **Missing references fail loudly.** Unassigned Input Action References or an unassigned `CameraTransform` log a descriptive error and disable the component instead of throwing exceptions every frame.
* **`Use Jump` / `Use Crouch` / `Use Dodge` now block the transition** instead of being checked after the state was entered. Disabling a mechanic in 1.0 could leave the player stuck in a state with no exit condition.
* **Idle decelerates** with `Movement Smoothing` instead of snapping to zero velocity.
* **Crouch and Dodge can exit into any locomotion state.** Standing up while sprinting now returns to Sprint, and a dodge that ends with no input returns to Idle.
* **`IsGrounded` no longer comes from `CharacterController.isGrounded`.** It is the result of the sphere-cast ground probe, which reports contact one frame earlier and does not flicker on slopes and ledges.
* **Vertical velocity is added to the movement vector rather than replacing its `Y`.** This is what lets slope-projected movement keep its downhill component. Custom states that wrote into `CurrentMoveVelocity.y` expecting it to be discarded must be reviewed.
* **`CharacterController.stepOffset` is written every frame** (zero while airborne, restored on landing) and **`minMoveDistance` is forced to `0` on `Awake`**.
* **A new state can be entered without any input:** `SlopeSlide` triggers purely from terrain. Custom states that assume every transition is input-driven should handle being interrupted by it.
* **An assembly definition was added** (`AZE.AdvancedFirstPerson`, auto-referenced). Code in `Assembly-CSharp` needs no changes; custom assembly definitions must reference it.
* **Pushed Rigidbodies keep their vertical velocity**, and `Weight Based Push` now actually scales the push by mass (it was unused in 1.0).
* **The generated input wrapper class was removed.** The asset reads input through `InputActionReference` only; if you used `InputSystem_Actions_Movement1` directly, re-enable *Generate C# Class* on the `.inputactions` asset in your own project.

---

## 7. Support

For bug reports, feedback, or questions regarding this asset, please contact:
**[studios.aze.contato@gmail.com]**
