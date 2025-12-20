# Sage Project - AI Coding Agent Instructions

## Project Overview
Sage is a protocol converter bridging OpenSimulator/SecondLife viewer protocols with the Basil protocol. Each Sage instance fronts a single OpenSimulator region, acting as a Basil space-server for that 256x256 region.

**Key architectural concept**: Viewers connect to multiple space-servers simultaneously (current region + up to 8 surrounding + potentially 16 beyond = 25+ servers), requiring Sage to handle multi-viewer connections efficiently.

## Project Structure

### Three-Component Architecture
- **Sage.Main**: Entry point console application with dependency injection setup ([Sage.Main/Main.cs](Sage.Main/Main.cs))
- **Sage.Comm**: Communication library for protocol handling (currently scaffolded)
- **Sage.Tests**: xUnit test suite with coverlet for coverage

All projects use `org.herbal3d.Sage` as the root namespace (not the default folder-based namespace).

### Documentation
Project uses **DocFX** for documentation generation:
- **docs/**: Markdown documentation source files
  - [docs/DesignNotes.md](docs/DesignNotes.md) - Core architectural decisions and design patterns
  - [docs/introduction.md](docs/introduction.md) - Project introduction
  - [docs/getting-started.md](docs/getting-started.md) - Getting started guide
- **_site/**: Generated documentation output (build artifact)
- **docfx.json**: DocFX configuration
- Documentation built from code XML comments and markdown files

## Development Workflows

### Building & Running
```bash
# Build entire solution
dotnet build Sage.sln

# Run main application
dotnet run --project Sage.Main/Sage.Main.csproj

# Run tests with coverage
dotnet test Sage.Tests/Sage.Tests.csproj /p:CollectCoverage=true

# Build documentation
docfx docfx.json
```

### Project Dependencies
- **Sage.Main** → **Sage.Comm**: Main application depends on communication library
- **Sage.Tests**: Independent test project
- **NuGet packages in Sage.Main**:
  - Microsoft.Extensions.Configuration (v10.0.1) - Configuration management
  - Microsoft.Extensions.DependencyInjection (v10.0.1) - DI container
  - Microsoft.Extensions.Logging (v10.0.1) - Logging infrastructure

## Critical Implementation Context

### Architecture Components (from docs/DesignNotes.md)

**Sage Block Diagram**: Multiple components work together:
- **Sage.Basil**: Handles multiple Basil viewer connections with independent authentication
- **Sage.Pool**: Central BItem store containing space entity metadata
- **Sage.OS**: OpenSimulator interface using libreMetaverse
- **Sage.Convert**: Asset conversion (OpenSimulator prims → GLTF)
- **Sage.Asset**: Asset storage and retrieval
- **Account Info Store**: Authentication/authorization data
- **Asset Store**: Large data storage (meshes, sounds, textures)

### Dual Protocol Handling

**OpenSimulator Side**: 
- Uses libreMetaverse library for region communication
- Converts objects to GLTF format via Sage.Convert
- Updates asset database and metadata
- Queries entity information and syncs to Sage.Pool (BItem store)

**Basil Side**:
- Manages multiple simultaneous viewer connections with independent auth
- Each connection negotiates its own transport/protocol (e.g., "WebTransport/MessagePack" or "WebSocket/JSON")
- Current implementation target: WebTransport + MessagePack
- Subscribes to BItem events in Sage.Pool for viewer updates

### BItem Store Architecture (Sage.Pool)
The core data structure representing space contents. BItems:
- Contain placement, motion, and references to displayables (NOT the actual mesh/sound data)
- Metadata includes position, ownership information
- Assets (mesh vertices, sounds, textures) stored separately in Asset Store
- Generate events that Basil subscribers consume for viewer updates
- Storage strategy TBD: in-memory with lazy DB writes vs periodic snapshots

**Open design question**: Whether Sage needs persistent storage at all, since OpenSimulator already dumps region data to viewers on startup.

### Service Layer Above Sage
A higher-level service manages Sage instances:
- Organizes creation of Sage instances (potentially lazy instantiation per region)
- Handles authentication/authorization for access to Sage instances
- May handle space-server discovery or assist with registration/discovery services

## Project-Specific Conventions

### Code Style
- **Brace style**: K&R (1TBS) - opening braces on same line
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
  - Classes accept dependencies in constructors
  - Service registration in `SageMain.RegisterServices()`
  - Enables unit testing of individual classes
- **Configuration**: Microsoft.Extensions.Configuration
  - Configuration sources: in-memory defaults, appsettings.json, Sage.json, environment variables (Sage_*), command line
- **Logging**: Microsoft.Extensions.Logging

### Naming & Organization
- Root namespace: `org.herbal3d.Sage` (explicitly set via `<RootNamespace>`)
- Target framework: .NET 9.0
- Nullable reference types enabled project-wide
- Implicit usings enabled

### Testing
- Framework: xUnit with Visual Studio runner
- Coverage: coverlet.collector for code coverage reporting
- Test files follow `UnitTest*.cs` pattern

## External References
- [Basil Protocol](https://herbal3d.org/BasilProtocol) - Target protocol specification
- [OpenSimulator](http://opensimulator.org) - Source protocol environment
- [SecondLife](https://secondlife.com) - Compatible viewer protocol
- libreMetaverse - Planned library dependency for OpenSimulator communication (not yet integrated)

## Current State
Project is in early implementation phase with:
- Dependency injection infrastructure in place (SageMain class)
- Configuration management setup
- DocFX documentation framework configured
- Core architectural components defined in design docs

**Key design decisions** documented in [docs/DesignNotes.md](docs/DesignNotes.md):
- Service layer above Sage instances for authentication and discovery
- Multi-connection management strategy for Basil viewers
- BItem store design (in-memory vs database tradeoffs)
- Distance queries for space-server aggregation
- Entity/asset separation (metadata in BItems, large data in Asset Store)

When implementing new features:
1. Refer to [docs/DesignNotes.md](docs/DesignNotes.md) for architectural context
2. Use dependency injection patterns established in SageMain
3. Follow K&R brace style conventions
4. Add XML documentation comments for DocFX generation
5. Prioritize protocol conversion components in Sage.Comm
