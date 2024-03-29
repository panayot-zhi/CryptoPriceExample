CREATE DATABASE IF NOT EXISTS crypto_price_example_db;

CREATE USER 'crypto_price_example_user'@'%' IDENTIFIED BY 'Admin123!';
GRANT SELECT, INSERT, UPDATE, DELETE, CREATE, DROP, REFERENCES, INDEX, ALTER, CREATE TEMPORARY TABLES, LOCK TABLES, EXECUTE, CREATE VIEW, SHOW VIEW, CREATE ROUTINE, ALTER ROUTINE, EVENT, TRIGGER ON `crypto_price_example_db`.* TO 'crypto_price_example_user'@'%';

FLUSH PRIVILEGES;
