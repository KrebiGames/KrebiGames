# Project: KrebiGames

## Purpose and platforms
Multiplayer Unity game using Photon Quantum.

## Verified baseline
- Unity: 6000.4.10f1
- ECS: **Quantum** (authoritative gameplay)
- Quantum SDK: `Packages/com.photonengine.quantum` (submodule `photon-quantum`)
- Gameplay: `Assets/QuantumUser` (submodule `quantum-krebi`)

## Project map
- Scenes: `Assets/Game/Scenes/`
- Quantum components: `Assets/QuantumUser/Simulation/Components/`
- Quantum systems: `Assets/QuantumUser/Simulation/Systems/`
- System registration: `Assets/QuantumUser/Simulation/SystemSetup.User.cs`, Quantum SystemsConfig
- Asset definitions/configuration: `Assets/QuantumUser/Simulation/Assets/`, `Assets/QuantumUser/Resources/`
- View and input: `Assets/QuantumUser/View/`
- Player controllers/UI: `Assets/Game/Player/`, `Assets/Game/UI/`

## Project-specific instructions
- Quantum simulation owns authoritative gameplay state.
- Use `.qtn` for Quantum component definitions.
- Configure gameplay through Quantum assets and entity prototypes.
- Use Quantum KCC for player movement.
- Runtime-only component fields use `[ExcludeFromPrototype]` where appropriate.
- Keep simulation logic separate from Unity presentation.
- Preserve existing submodule architecture.

## Verified checks
- Dependencies: `git submodule update --init --recursive`
- Compilation: Unity Editor Console
- Gameplay: Play Mode, player spawning, movement and interactions
- Automated tests/scenarios: not yet verified
- Target builds: Standalone
