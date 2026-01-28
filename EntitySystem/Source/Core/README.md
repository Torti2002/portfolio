# Entity System – Core Architecture

This folder contains the **core building blocks** of the Entity System.
The goal of this architecture is to provide a **clear separation between data, behavior, and runtime state**, without relying on Unity ECS or heavy reflection-based systems.

The system is designed to be **explicit, readable, and easy to reason about**, even as project complexity grows.

---

## 🧠 Design Goals

- Clear separation of **logic** and **state**
- Data-driven entities that are easy to save, load, and replicate
- Minimal magic, no hidden runtime behavior
- Easy to extend without modifying existing code
- Suitable for small to mid-sized Unity projects

---

## 🧱 Core Concepts

### 1. Entity

An `Entity` represents a **runtime object** in the game world.

- Owns and manages a set of components
- Acts as the communication hub between components
- Does **not** store persistent state itself

Think of an Entity as the *container that brings behavior and data together at runtime*.

---

### 2. EntityComponent

`EntityComponent` is the base class for all behavior.

- Encapsulates logic (movement, health, inventory, visuals, etc.)
- Can optionally:
  - Receive configuration data
  - Read from and write to an EntityGhost
- Components are intentionally decoupled from each other

Components **do not own persistent data** – they operate on state provided to them.

This makes components:
- Reusable
- Easier to test
- Easier to refactor

---

### 3. EntityGhost

`EntityGhost` is a **pure data container** that represents the current state of an Entity.

Typical use cases:
- Saving / loading
- Networking / replication
- Snapshots or rollback systems

Key characteristics:
- Contains **no logic**
- Serializable
- Can exist independently of the Entity at runtime

The EntityGhost acts as a *single source of truth* for the entity's state.

---

## 🔌 Interfaces & Data Flow

Behavior is opt-in via small, focused interfaces, for example:

- Components that need configuration data
- Components that need access to persistent state

This avoids:
- Large base classes
- Forced inheritance
- Tight coupling between systems

Each component explicitly declares **what it needs**, nothing more.

---

## 🔁 Typical Runtime Flow

1. An `Entity` is created
2. Components are attached
3. An `EntityGhost` is assigned or generated
4. Components read and modify state through the ghost
5. The Entity acts as the coordinator between systems