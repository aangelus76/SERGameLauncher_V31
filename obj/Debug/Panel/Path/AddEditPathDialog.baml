   M S B A M L   `   `   `  ����O  KSERGamesLauncher_V31, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null;"clr-namespace:SERGamesLauncher_V31SERGamesLauncher_V31  5   	   ,    &SERGamesLauncher_V31.AddEditPathDialog   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   4local"clr-namespace:SERGamesLauncher_V31     5   	     � Title$%  Ajouter un chemin d'application��5	   	   $	��320��5
   	   $	��550��5   	    � WindowStyle$
 None=�5   	    � 
ResizeMode$ NoResize=�5   	   
������  � AllowsTransparency .  � WindowStartupLocation$ CenterOwner=�5   	   .��5      ��  �� CornerRadius$ 15��6   ���#474747._�5   
   � $��2q�6    � RowDefinitions 5      �� 5      $��40 �6    �� 5      $��* �6    �� 5      $��60 �6    .+�5      �� $��0��6   ���#171717$ 	12,12,0,0��67   ._�5      � .+�5      ��  �� Orientation$ 
Horizontal=�6!   $��Center=�6:   $��10,0,0,0q�6U   .+�5      �� $>��8/SERGamesLauncher_V31;component/Images/IconeLauncher.ico��6    $��20��6b   $��20��6m   $��0,0,8,0q�6y   �� 5      -   5         ��Name$ titleTextBlock��5   $   $%��Ajouter un chemin d'application��6<   
�������$��16(�6v   $
��Bold"�6�   5       �� 5"      -   5        	 ��Name$	 btnClose��5"         IconButtonStyle#��[  6/   
 � ToolTip$
 Fermer��5#      $��Right=�6.   $��0,0,10,0q�6J   .��5%      �� -   5         ��Name$ 
closeImage��5%         ForceNormal#��[ 64   $��26��6Z   $��26��6e    � Triggers 5&      :� 5'       :� RoutedEvent$ Button.MouseEnter��6+   .[�5(   "   �� .��5)   &   �� .��5*   *   T�  �� 
TargetName$ 
closeImage��6H    �� TargetProperty$ Source�6k   ..�5+   .   i�  R� KeyTime$ 0:0:0��6E     	ForceOver R� Value# [ 6U   5,   +   5-   '   5.   #   5/      :� 50      $ Button.MouseLeave��6+   .[�51   "   �� .��52   &   �� .��53   *   T� $ 
closeImage��6H   $ Source�6k   ..�54   .   i� $ 0:0:0��6E   # [ 6U   55   +   56   '   57   #   58      5:      5;      5<      � 5?      $��1��6   $��20,15q�6     5@      �� 5A      $
��Auto �6$   �� 5B      $
��Auto �6$   �� 5C      $
��Auto �6$   �� 5D      $
��Auto �6$    � ColumnDefinitions 5F      �� 5G      $	��130 �6'   �� 5H      $��* �6'   �� 5I      $��78.977 �6'   �� 5J      $��13.953 �6'   �� 5K      $
��7.07 �6'   .+�5O      �� $��0��6   $��Plateforme :��6,   
�������$��14(�5P      $��Center=�6*   �� 5Q      -   5         ��Name$ cmbPlatform��5Q   L   $��0��6   $��1��6(   $��4��68   ���#333333���#333333���#444444$��1q�5S       $��30��5T   
   $��5q�6   $��Center=�6"   $��0,5,0,5q�5U   
    � 	Resources 5V       �� Color��%�   (��       5W   *   *SystemColors.WindowBrushKey+)(�� !     5X   *   * SystemColors.HighlightBrushKey+)�� 5W      $ #333333��6Y   �� 5X      $ #268531��6\   5Y      .@�5Z      �� $��Steam��6#   
�������63   �� 5[      $
��Epic��6#   
�������62   �� 5\      $��Roblox��6#   
�������64   �� 5]      $
��Xbox��6#   
�������62   �� 5^      $��Crazy��6#   
�������63   �� 5_      $	��BGA��6#   
�������61   �� 5`      $��Custom��6#   
�������64   5a      �� 5d      $��1��6   $��0��6)   $��Type de chemin :��69   
�������$��14(�5e      $��Center=�6*   �� 5f      $��1��6   $��1��6*   $��4��6:   $ 
Horizontal=�6N   $��0,5,0,5q�6g   .+�5g      	� -   5         	�Name$ rbLocal��5g   "   $��Fichier local��63   
������� d� 	IsChecked$
 TrueY�5h   "   $��0,0,15,0q�63   $��Center=�6E   5i   "   	� 5j      -   5        $ rbUrl��5j   "   $��URL (site web)��61   
�������$��Center=�5k   "   5l   "   5m      �� 5p      $��2��6   $��0��6)   $��Chemin :��69   
�������$��14(�5q      $��Center=�6*   �� 5r      -   5         ��Name$ txtPath��5r   7   $��2��6   $��1��6'     AdminTextBoxStyle#��[ 5s      $��0,5,67,5q�6E   �� 5t      -   5        $	 	btnBrowse��5u      $��2��5t      $��1��6&   $��	Parcourir��66   $��	225,5,1,5q�6J   $��30��6]     LaunchButtonStyle#��[ 5v      $��4��5w   1   �� 5z      $��3��6   $��0��6)   $��Arguments :��69   
�������$��14(�5{      $��Center=�6*   �� 5|      -	   5        $ txtArguments��5|   K   $��3��6   $��1��6'   $��4��67   #��[ 5}      $��0,5,0,5q�6E   5~      � 5�      $��2��6   $��0,0,0,15q�6     5�      �� 5�      $��* �6'   �� 5�      $��* �6'   .+�5�      �� -
   5        $��0��5�      $��Annuler��6)   #��[ 5�      $	��120��5�      $��40��6%   $��0,0,5,0q�61   $��Right=�6B   �� 5�      -   5        $��1��5�      $��Valider��6)   #��[ 5�      $	��120��5�      $��40��6%   $��5,0,0,0q�61   $
��Left=�6B   5�      5�      5�      5�      