DROP DATABASE IF EXISTS AutoCar;
CREATE DATABASE AutoCar;
USE AutoCar;

DROP TABLE IF EXISTS login;
DROP TABLE IF EXISTS logs;
DROP TABLE IF EXISTS vendas;
DROP TABLE IF EXISTS vendedores;
DROP TABLE IF EXISTS clientes;
DROP TABLE IF EXISTS carros;

CREATE TABLE IF NOT EXISTS login (
    id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    pass VARCHAR(255),
    nivel ENUM('admin', 'user') NOT NULL DEFAULT 'user',
    lastlogin DATETIME
);

INSERT INTO login(username, pass, nivel) VALUES ('root','1234','admin'), ('user','1234','user');

CREATE TABLE IF NOT EXISTS logs (
    id INT AUTO_INCREMENT PRIMARY KEY,
    timestamp_log TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    tipo ENUM("Erro","Info"),
    msg TEXT
);

CREATE TABLE IF NOT EXISTS vendedores (
    id_vendedor INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(50) NOT NULL,
    email VARCHAR(250) NOT NULL UNIQUE,
    telefone VARCHAR(15) NOT NULL UNIQUE,
    imagem LONGBLOB
);
INSERT INTO vendedores (nome, email, telefone) VALUES
('Márcio Carvalho', 'naodigo@hotmail.com', 999999999);

CREATE TABLE IF NOT EXISTS clientes (
    id_cliente INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(50) NOT NULL,
    endereco VARCHAR(250),
    email VARCHAR(250) NOT NULL UNIQUE,
    telefone VARCHAR(15) NOT NULL UNIQUE
);

INSERT INTO clientes (nome, endereco, email, telefone) VALUES
('Rui', 'Rua de alguma coisa', 'rui@hotmail.com', 999999999);

CREATE TABLE IF NOT EXISTS carros (
    id_carro INT AUTO_INCREMENT PRIMARY KEY,
    marca VARCHAR(50) NOT NULL,
    modelo VARCHAR(50) NOT NULL,
    cilindrada INT NOT NULL, 
    potencia INT NOT NULL,
    tipo_combustivel ENUM('Gasolina', 'Gasóleo', 'GPL', 'Híbrido', 'Elétrico') NOT NULL,
    imagem LONGBLOB,
    vendido varchar(3) DEFAULT 'Não',
    preco DECIMAL(10,2) NOT NULL CHECK (preco > 0)
);

INSERT INTO carros (marca, modelo, cilindrada, potencia, tipo_combustivel, imagem, vendido, preco) VALUES
('BMW', 'M3 Competition', 3000, 510, 'Gasolina', NULL, 'Não', 95000.00),
('Mercedes-Benz', 'C 220d', 2000, 200, 'Gasóleo', NULL, 'Não', 55000.00),
('Toyota', 'Corolla Hybrid', 1800, 122, 'Híbrido', NULL, 'Não', 32000.00),
('Tesla', 'Model 3 Long Range', 0, 351, 'Elétrico', NULL, 'Não', 60000.00),
('Volkswagen', 'Golf 8 GTI', 2000, 245, 'Gasolina', NULL, 'Não', 45000.00),
('BMW', 'M3 Competition', 3000, 510, 'Gasolina', NULL, 'Não', 95000.00);

CREATE TABLE IF NOT EXISTS carros_vendidos (
    id_carro INT PRIMARY KEY,
    marca VARCHAR(50) NOT NULL,
    modelo VARCHAR(50) NOT NULL,
    cilindrada INT NOT NULL CHECK (cilindrada > 0), 
    potencia INT NOT NULL CHECK (potencia > 0),
    tipo_combustivel ENUM('Gasolina', 'Gasóleo', 'GPL', 'Híbrido', 'Elétrico') NOT NULL,
    imagem LONGBLOB,
    vendido varchar(3) DEFAULT 'Sim',
    preco DECIMAL(10,2) NOT NULL CHECK (preco > 0)
);

CREATE TABLE IF NOT EXISTS vendas (
    id_venda INT AUTO_INCREMENT PRIMARY KEY,
    nome_cliente VARCHAR(50) NOT NULL,
    nome_vendedor VARCHAR(50) NOT NULL,
    modelo_carro VARCHAR(50) NOT NULL,
    preco_venda DECIMAL(10,2) NOT NULL CHECK (preco_venda > 0),
    data_venda DATE NOT NULL DEFAULT (CURDATE()),
    hora_venda TIME NOT NULL DEFAULT (CURTIME())
);