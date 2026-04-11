# SportsLeague

## PostgreSQL con Docker

Levantar la base de datos:

```bash
docker compose up -d
```

Detener la base de datos:

```bash
docker compose down
```

La API usa esta conexion en desarrollo:

```text
Host=localhost;Port=5432;Database=sportsleague_db;Username=sportsleague_user;Password=sportsleague_password
```
