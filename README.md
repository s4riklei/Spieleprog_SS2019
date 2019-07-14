# Spieleprog_SS2019

### Inhalt
- Ordner BlockEngine: Das Unity-Projekt
- Ordner CubesClient: Der Quellcode für den minimalen Client
- Ordner CubesServer: Der Quellcode für den erweiterten Server
- Ordner Builds: Einen fertigen Build des Unity-Projektes, sowie .jar-Dateien von Server und Client

### Verwendung CubesServer
An der Verwendung des Servers hat sich nichts verändert, dieser erwartet nach wie vor nur eine Portnummer als Argument

### Verwendung CubesClient
Der minimale Client erwartet 5 Argumente:
- Serveradresse
- Port
- Verzögerung zwischen Positionsupdate-Nachrichten der Clients (in ms)
- Verzögerung zwischen Blockupdate-Nachrichten der Clients (in ms)
- Anzahl der zu instanziierenden Clients

Beispiel:
```
java -jar CubesServer localhost 56789 50 100 3
```

### BlockEngine
Steuerung:
- W,A,S,D: Bewegung
- Leertaste: nach oben bewegen
- C: nach unten bewegen
- Mausrad: durch Blocktexturen wechseln
- linke Maustaste: Block setzen/entfernen
- Q: Grid einblenden
- E: Blockplatzierungshilfe einblenden
- F: "Taschenlampe"
- Tab: Konsole öffnen

Konsole:
Der Befehl "?" oder "help" gibt eine Liste der möglichen Befehle aus