   M S B A M L   `   `   `  ����L  HCSGOCheatDetected, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullK*clr-namespace:CSGOCheatDetector.ViewModelsCSGOCheatDetector.ViewModels  5      +    %CSGOCheatDetector.Views.FileCheckPage   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      9vm*clr-namespace:CSGOCheatDetector.ViewModels     5        N� Title$  FileCheckPage��5       � DataContext 5      5   /CSGOCheatDetector.ViewModels.FileCheckViewModel  5   
   .��5      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   ��  �� Orientation$ 
Horizontal=�6   $��Center=�6.   $��10q�6K   .+�5      �� -   5         ��Name$ SearchButton��5      $��
Поиск��6+      NavButtonStyle#��[  6]   $
��10,5q�6�   $��5q�6�   ��6;   �� *SearchCommand+6�   �� 5      -   5        $ 
StopButton��5      $��Стоп��6)   #��[  6X   $
��10,5q�6�   $��5q�6�   ��68   �� *StopCommand+6�   �� 5      -   5        $ 
SaveButton��5      $��Сохранить��6)   #��[  6]   $
��10,5q�6�   $��5q�6�   ��6=   �� *SaveCommand+6�   5      �� 5   
   $��1��6   $
��Auto=�6$   $
��Auto=�6I   $��10q�6l   .��5      &   System.Windows.Controls.DataGrid  -   5          Name$ dataGridResults��5         AutoGenerateColumns .     
IsReadOnly . ��6^   �� *SuspiciousFiles+   Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn	   Header$	 Имя��6)   
   Width5  /System.Windows.Controls.DataGridLengthConverter$	
 200 6O   1  +System.Windows.Controls.DataGridBoundColumn   Binding 66   �� *Name+6O     5      $	 Размер (KB)��6)   $	
 100 6W    6>   �� *Size+6W     5      $	 Дата создания��6)   $	
 150 6a    6@   �� *CreationDate+6a     5      $!	 Дата изменения��6)   $	
 150 6f    6A   �� *ModificationDate+6f     5      $	 Дата доступа��6)   $	
 150 6^    6?   �� *
AccessDate+6^     5      $	 Полный путь��6)   $	
 400 6[    6>   �� *
FullPath+6[   5       5!      �� 5"   
   -   5         ��Name$ labelStatus��5"      $��2��6)   $��Center=�6N   $��10q�6k   $��14(�6w   ��66   �� *Status+6w   5#      5$      