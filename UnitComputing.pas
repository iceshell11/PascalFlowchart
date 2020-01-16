unit UnitComputing;

interface

uses 
  UnitTypes;

{ Функция поиска суммы положительных элементов матрицы, расположенных выше главной диагонали }
function Sum(const x:matrix;n:integer):real;
implementation

function Sum(const x:matrix;n:integer):real;
var i,j:integer;
begin
  Result:=0;
  for i := 1 to n do begin
    for j := 1 to n do begin
      if ((x[i,j] < 0) and (i <> j)) then begin
        Result:=Result+x[i,j];
      end;
    end;
  end;
end;

end.