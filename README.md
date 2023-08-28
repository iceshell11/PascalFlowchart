# PascalFlowchart
Утилита для построения графов из кода на языке Pascal разработанная на Unity

Скачать последнюю версию можно здесь: https://github.com/iceshell11/PascalFlowchart/releases

Пример:

![Alt-текст](https://i.ibb.co/jTv12mB/1.png "Пример")

Код программы на языке Pascal можно загрузить через соответствуещее текстовое поле или из буфера обмена.
Если в коде содержатся операторы помимо:
- program
- begin
- end
- if
- else
- for
- until
- while
- do
- var
- type
- function
- procedure
- except
- uses
- repeat
- const
- initialization
- finalization

скорее всего результат будет далек от нужного, но для небольших программ этого обычно хватает.

Если программа имеет модули они тоже будут отображены на схеме если расположить их файлы в той же директории, что и исполняемый файл PascalFlowchart (для Windows)

Также реализована возможность изменить некоторые параметры генерации и сохранить схему в png файл.
