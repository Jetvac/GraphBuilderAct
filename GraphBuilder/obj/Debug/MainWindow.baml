   M S B A M L   `   `   `  ����G  CGraphBuilder, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:GraphBuilderGraphBuilder  5   	       GraphBuilder.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   ,localclr-namespace:GraphBuilder     5   	   -   5          � Title$  
MainWindow��5   	   $	��450��6   $	��800��6)   .��5	      �  � ColumnDefinitions 5
   
   �� 5      �� 5      $	��200 �6   .+�5   
   �� -   5         ��Name$ GraphicRender��5      
�������._�5      � .+�5      �� -   5         ��Name$ 
EdgeRender��5      5      �� 5      -   5        $ WeightRender��5      5      �� 5      -   5        $ 
NodeRender��5      5      5      5      �� 5   
   $��1��6   
�������$��2 0 0 0q�68   
����ح�._�5      �� -   5         ��Name$ UserActiveController��5      $	��Top=�67   .+�5       �� -   5         ��Name$ AddNodeSwitch��5       $��5q�60   $��0��6;   .��6d   Добавить узел6s   �� 5!      -   5        $ DeleteNodeSwitch��5!      $��5q�63   $��2��6>   .��6j   Удалить узел6x   �� 5"      -	   5        $ AddEdgeSwitch��5"      $��5q�60   $��1��6;   .��6d   #!Соединить вершины6w   �� 5#      -
   5        $ DefaultModeSwitch��5#      $��5q�64   $��2��6?   .��6l   По умолчанию6z   5$      5%      5&      5'      