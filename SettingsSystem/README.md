# SettingsSystem

Dieses Modul zeigt ein datengetriebenes und erweiterbares
Settings-System in C#, das unabhängig von einer konkreten
UI oder Engine aufgebaut ist.

## Architektur

Die Kernlogik ist bewusst von Darstellung und Plattform getrennt.
Settings werden über Definitionen beschrieben und können
serialisiert, validiert und ausgewertet werden.

UI-spezifische Implementierungen sind ausgelagert und dienen
ausschließlich als Beispiele.

## Struktur

- Core  
  Zentrale Logik zur Verwaltung und Auswertung von Settings.

- Definitions  
  Datenstrukturen zur Beschreibung einzelner Einstellungen
  (z. B. Typ, Wertebereich, Metadaten).

- Serialization  
  Laden und Speichern von Settings aus JSON.

- Examples  
  Beispielhafte Integrationen des Systems.

  - Unity  
    UI-Widgets zur Darstellung und Bearbeitung von Settings
    innerhalb von Unity.
