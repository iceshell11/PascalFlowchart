unit UnitInputOutput;

{$I-}  

interface

uses 
  UnitTypes;

var
  fin, fout: TextFile;

{ Процедура ввода матрицы из файла }
procedure Get(var x: matrix; var n: integer; var f: TextFile);

implementation

{ Процедура ввода матрицы из файла }
procedure Get(var x: matrix; var n: integer; var f: TextFile);
var
  i, j: integer;
begin
  readln(f, n);
  for i := 1 to n do
  begin
    for j := 1 to n do
      read(f, x[i, j]);
    readln(f);
  end;
end;


initialization

  if ParamCount < 2 then
  begin
    writeln('There are no enough parameters');
    readln;
    halt;
  end;
  AssignFile(fin, ParamStr(1));
  Reset(fin);
  if IOResult <> 0 then
  begin
    writeln('It is not possible to open file ''', ParamStr(1), ''' for reading');
    readln;
    halt;
  end;

  AssignFile(fout, ParamStr(2));
  Rewrite(fout);
  if IOResult <> 0 then
  begin
    writeln('It is not possible to open file ''', ParamStr(2), ''' for writing');
    writeln('Output is made to the screen. Press ENTER');
    readln;
    AssignFile(fout, '');
    Rewrite(fout);
  end;

finalization

  CloseFile(fin);
  CloseFile(fout);
  IOResult;

end.