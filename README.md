# zeitmessung_backend
## Starten des Servers
1. Verbindung zum Server herstellen `ssh me@172.18.2.16`
2. `cd /zeitmessung-backend`
3. `tmux` ermöglicht das beenden der ssh-Sitzung ohne Beendigung des Servers
4. `su`
5. `sh start_backend.sh &` im Projektroot ausführen (& führt dazu, dass das File in einem eigenen Prozess ausgeführt wird)
6. hf&gl :)

## Updaten der Dokumentation
1. installiere doxygen http://www.doxygen.nl/
2. `doxygen backend_documentation.conf`im Projektroot ausführen
3. [Projektroot]/Documentation/Code/index.html im Browser öffnen
