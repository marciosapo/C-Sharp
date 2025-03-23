drop table if exists login;
create table if not exists login(
	id int AUTO_INCREMENT Primary key,
    username varchar(50),
    userpass varchar(50),
    email varchar(50),
    lastlogin dateTime
);

insert into login (username, userpass, email, lastlogin) values('root', 'root', 'm.DR.carvalho@hotmail.com', null);

drop table if exists login_logs;
CREATE TABLE IF NOT EXISTS login_logs (
    id INT AUTO_INCREMENT PRIMARY KEY,
    data_login DATE,
    hora_login TIME,
    login_tipo varchar(50)
);

CREATE TRIGGER login_sucesso
AFTER UPDATE ON login
FOR EACH ROW
BEGIN
    INSERT INTO login_logs (data_login, hora_login, login_tipo)
    VALUES (CURRENT_DATE, CURRENT_TIME, "Login Sucesso");
END;
