   M S B A M L   `   `   `  ����O  KSERGamesLauncher_V31, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null;"clr-namespace:SERGamesLauncher_V31SERGamesLauncher_V31  5   	   0    *SERGamesLauncher_V31.ProcessSelectorDialog   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   4local"clr-namespace:SERGamesLauncher_V31     5   	     � Title$  Sélection d'un processus��5   	   $	��500��5	   	   $	��600��5
   	    � WindowStyle$
 None=�5   	    � 
ResizeMode$ NoResize=�5   	   
������  � AllowsTransparency .  � WindowStartupLocation$ CenterOwner=�5   	   .��5      ��  �� CornerRadius$ 15��6   ���#474747._�5   
   � $��2q�6    � RowDefinitions 5      �� 5      $��40 �6    �� 5      $
��Auto �6    �� 5      $��* �6    �� 5      $��60 �6    .+�5      �� $��0��6   ���#171717$ 	12,12,0,0��67   ._�5      � .+�5      ��  �� Orientation$ 
Horizontal=�6!   $��Center=�6:   $��10,0,0,0q�6U   .+�5      �� $>��8/SERGamesLauncher_V31;component/Images/IconeLauncher.ico��6    $��20��6b   $��20��6m   $��0,0,8,0q�6y   �� 5      $��Sélection d'un processus��6$   
�������$��16(�6W   $
��Bold"�6e   5       �� 5"      -   5         ��Name$ btnClose��5"         IconButtonStyle#��[  6/   	 � ToolTip$	 Fermer��5#      $��Right=�6.   $��0,0,10,0q�6J   .��5%      �� -   5        
 ��Name$
 
closeImage��5%         ForceNormal#��[ 64   $��26��6Z   $��26��6e    � Triggers 5&      :� 5'       :� RoutedEvent$ Button.MouseEnter��6+   .[�5(   "   �� .��5)   &   �� .��5*   *   T�  �� 
TargetName$ 
closeImage��6H    �� TargetProperty$ Source�6k   ..�5+   .   i�  R� KeyTime$ 0:0:0��6E     	ForceOver R� Value# [ 6U   5,   +   5-   '   5.   #   5/      :� 50      $ Button.MouseLeave��6+   .[�51   "   �� .��52   &   �� .��53   *   T� $ 
closeImage��6H   $ Source�6k   ..�54   .   i� $ 0:0:0��6E   # [ 6U   55   +   56   '   57   #   58      5:      5;      5<      � 5?      $��1��6   $��
20,15,20,5q�6     � ColumnDefinitions 5@      �� 5A      $
��Auto �6'   �� 5B      $��* �6'   .+�5E      �� $��0��6   $��Rechercher :��6,   
�������$��14(�5F      $��Center=�6*   $��0,0,10,0q�6E   �� 5H      -   5         ��Name$ 	txtSearch��5H   *   $��1��6     AdminTextBoxStyle#��[ 5I      $��30��5J      5K      �� 5N      $��1��6   $;��5Double-cliquez sur un processus pour le sélectionner��6%   
�������$��14(�5O   +   $��Center=�69   $��0,55,0,0q�5P      �� 5S      -   5         ��Name$ processListView��5S   $   $��2��6   ���#1E1E1E
����������#444444$��1q�5T   @   $��
20,10,20,0q�5V       �� 
GroupStyle 5W      (  "System.Windows.Controls.GroupStyle  5X         HeaderTemplate 5Y      �� 5Z      .R�5[   "   �� $
��Bold"�6B   $��16(�5\   ,   ���#268531$	��0,5q�5]   ,   ��5[   ,   �� *Name+5]   ,   5^      5`      ��5c      �� 5d      .R�5e      �  5f      �� 5g   "   $	��200 �63   �� 5h   "   $��* �63   �� 5i   "   $
��Auto �63   .+�5l      �� $��0��6(   
�������$
��Bold"�5m   ;   ��5l   8   �� *Name+5m   ;   �� 5o      $��1��6(   ���#AAAAAA$��CharacterEllipsis=�5p   =   ��5o   8   �� *WindowTitle+5p   =   �� 5r      $��2��6(   ���#268531$��5,0,0,0q�5s   >   ��5r   8   �� *InstanceCount+ �� StringFormat$ ({0})��y�5t   )   �� *InstanceCount+  BoolToVisibilityConverter �� 	Converter# [  �� ConverterParameter$	 gt1��5u      5v      5x      � 5{      $��3��6   $��0,0,0,15q�6     5|      �� 5}      $
��Auto �6'   �� 5~      $��* �6'   �� 5      $
��Auto �6'   �� 5�      $
��Auto �6'   .+�5�      �� -   5        $��0��5�      $��Rafraîchir��6)     LaunchButtonStyle#��[ 5�      $	��120��5�      $��40��6%   $��20,0,0,0q�61   $
��Left=�5�      5�      �� 5�      -   5        $��2��5�      $��Annuler��6)   #��[ 5�      $	��120��5�      $��40��6%   $��0,0,10,0q�61   $��Right=�5�      5�      �� 5�      -   5        $��3��5�      $��Sélectionner��6)   #��[ 5�      $	��120��5�      $��40��6%   $��0,0,20,0q�61   $��Right=�5�      5�      5�      5�      5�      5�      