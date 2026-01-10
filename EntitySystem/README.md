# EntitySystem

Dieses Modul zeigt ein modulares Entity-Component-System in C#.
Der Fokus liegt auf Erweiterbarkeit, klarer Verantwortlichkeit
und loser Kopplung der einzelnen Bestandteile.

## Architektur

Das System ist in einen engine-agnostischen Core und optionale
Beispiele unterteilt. Die Kernlogik ist unabhängig von einer
konkreten Spiel- oder UI-Implementierung aufgebaut.

## Struktur

- Core  
  Zentrale Klassen und Abstraktionen wie Entities, Components
  und EntityGhosts.

- Components  
  Basisklassen und Mechaniken zur Erweiterung von Entities
  über Komponenten.

- Serialization  
  Gemeinsame Serialisierungslogik für Entity-Zustände.

- Examples  
  Ausgewählte Beispiel-Komponenten zur Demonstration der Nutzung
  des Systems. Diese sind nicht Teil der Kernlogik.
