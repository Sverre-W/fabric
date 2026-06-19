SELECT 'CREATE DATABASE fabric OWNER "user"'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'fabric')\gexec

SELECT 'CREATE DATABASE keycloak OWNER "user"'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'keycloak')\gexec
