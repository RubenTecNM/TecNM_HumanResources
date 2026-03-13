-- Script para eliminar marcadores de futuro de la tabla Corte
-- Recomendación: ejecutar en entorno de staging primero y hacer respaldo de la tabla Corte

BEGIN TRANSACTION;

-- Revisar cuántos registros serían eliminados
SELECT COUNT(*) AS MarcadoresEncontrados FROM Corte WHERE Inicia IS NULL AND Termina IS NULL;

-- Opcional: respaldar los registros encontrados en una tabla temporal
SELECT * INTO Corte_Marcadores_Backup FROM Corte WHERE Inicia IS NULL AND Termina IS NULL;

-- Eliminar los marcadores (registros con Inicia y Termina NULL)
DELETE FROM Corte WHERE Inicia IS NULL AND Termina IS NULL;

COMMIT TRANSACTION;

-- Nota: Los triggers existentes no requieren cambios para esta operación, pero revise
-- cualquier procedimiento almacenado o lógica en producción que asumiera la existencia
-- de estos marcadores y actualícelos en consecuencia.


select * from Usuario 