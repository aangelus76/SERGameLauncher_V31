   M S B A M L   `   `   `  ����O  KSERGamesLauncher_V31, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null;"clr-namespace:SERGamesLauncher_V31SERGamesLauncher_V31  5   	   .    (SERGamesLauncher_V31.AddEditFolderDialog   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   4local"clr-namespace:SERGamesLauncher_V31     5   	     � Title$"  Ajouter un dossier protégé��5	   	   $	��320��5
   	   $	��550��5   	    � WindowStyle$
 None=�5   	    � 
ResizeMode$ NoResize=�5   	   
������  � AllowsTransparency .  � WindowStartupLocation$ CenterOwner=�5   	   .��5      ��  �� CornerRadius$ 15��6   ���#474747._�5   
   � $��2q�6    � RowDefinitions 5      �� 5      $��40 �6    �� 5      $��* �6    �� 5      $��60 �6    .+�5      �� $��0��6   ���#171717$ 	12,12,0,0��67   ._�5      � .+�5      ��  �� Orientation$ 
Horizontal=�6!   $��Center=�6:   $��10,0,0,0q�6U   .+�5      �� $>��8/SERGamesLauncher_V31;component/Images/IconeLauncher.ico��6    $��20��6b   $��20��6m   $��0,0,8,0q�6y   �� 5      -   5         ��Name$ titleTextBlock��5   $   $"��Ajouter un dossier protégé��6<   
�������$��16(�6q   $
��Bold"�6   5       �� 5"      -   5        	 ��Name$	 btnClose��5"         IconButtonStyle#��[  6/   
 � ToolTip$
 Fermer��5#      $��Right=�6.   $��0,0,10,0q�6J   .��5%      �� -   5         ��Name$ 
closeImage��5%         ForceNormal#��[ 64   $��26��6Z   $��26��6e    � Triggers 5&      :� 5'       :� RoutedEvent$ Button.MouseEnter��6+   .[�5(   "   �� .��5)   &   �� .��5*   *   T�  �� 
TargetName$ 
closeImage��6H    �� TargetProperty$ Source�6k   ..�5+   .   i�  R� KeyTime$ 0:0:0��6E     	ForceOver R� Value# [ 6U   5,   +   5-   '   5.   #   5/      :� 50      $ Button.MouseLeave��6+   .[�51   "   �� .��52   &   �� .��53   *   T� $ 
closeImage��6H   $ Source�6k   ..�54   .   i� $ 0:0:0��6E   # [ 6U   55   +   56   '   57   #   58      5:      5;      5<      � 5?      $��1��6   $��20,15q�6     5@      �� 5A      $
��Auto �6$   �� 5B      $
��Auto �6$   �� 5C      $
��Auto �6$   �� 5D      $
��Auto �6$    � ColumnDefinitions 5F      �� 5G      $	��130 �6'   �� 5H      $��* �6'   �� 5I      $��80 �6'   .+�5M      �� $��0��6   $��0��6)   $��Nom :��69   
�������$��14(�5N      $��Center=�6*   �� 5O      -   5         ��Name$ txtName��5O   K   $��0��6   $��1��6'   $��2��67     AdminTextBoxStyle#��[ 5P      $��0,5,0,5q�6E   �� 5S      $��1��6   $��0��6)   $��Chemin :��69   
�������$��14(�5T      $��Center=�6*   �� 5U      -   5        $ txtFolderPath��5U   7   $��1��6   $��1��6'   #��[ 5V      $��0,5,5,5q�6E   �� 5W      -   5        $��1��5W      $��2��6&   $��	Parcourir��66     LaunchButtonStyle#��[ 5X      $��30��5Y      $��70��6%   $��0,5,0,5q�60   5Z      �� 5]      $��2��6   $��0��6)   $��Protection :��69   
�������$��14(�5^      $��Center=�6*   �� 5_      -   5         ��Name$ cmbProtectionLevel��5_   L   $��2��6   $��1��6(   $��2��68   ���#333333
����������#444444$��1q�5a   1   $��30��5b      $��5q�6'   $��Center=�63   $��0,5,0,5q�5c       � 	Resources 5d       �� Color��%�   (��       5e   *   *SystemColors.WindowBrushKey+)(�� !     5f   *   * SystemColors.HighlightBrushKey+)�� 5e      $ #333333��6Y   �� 5f      $ #268531��6\   5g      .@�5h      �� $5��/Lecture seule (bloque écriture et suppression)��6#   
�������
 � Tag$ ReadOnly��6o   �� 5i      $)��#Empêcher la suppression uniquement��6#   
�������$ PreventDeletion��6c   �� 5j      $1��+Empêcher la création de fichiers/dossiers��6#   
�������$ PreventCreation��6j   5k      �� 5n      $��3��6   $��0��6)   $��Au démarrage :��69   
�������$��14(�5o      $��Center=�6*   �� 5p      -   5         ��Name$ chkEnableOnStartup��5p   L   $��3��6   $��1��6(   $��2��68   $:��4Activer la protection au démarrage de l'application��5q       d� 	IsChecked$
 TrueY�5r      $��Center=�6,   
�������$��14(�5s   .   $��0,5,0,5q�5t      5u      � 5x      $��2��6   $��0,0,0,15q�6     5y      �� 5z      $��* �6'   �� 5{      $��* �6'   .+�5~      �� -	   5        $��0��5~      $��Annuler��6)   #��[ 5      $	��120��5�      $��40��6%   $��0,0,5,0q�61   $��Right=�6B   �� 5�      -
   5        $��1��5�      $��Valider��6)   #��[ 5�      $	��120��5�      $��40��6%   $��5,0,0,0q�61   $
��Left=�6B   5�      5�      5�      5�      