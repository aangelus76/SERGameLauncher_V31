   M S B A M L   `   `   `  ����O  KSERGamesLauncher_V31, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null;"clr-namespace:SERGamesLauncher_V31SERGamesLauncher_V31  5      3    -SERGamesLauncher_V31.FolderPermissionsControl   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      4local"clr-namespace:SERGamesLauncher_V31     5      .��5
      �   � RowDefinitions  5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $��0��6   $��Permissions des dossiers��6!   
�������$��24(�5   '   $
��Bold"�65   $��Center=�5      $��	0,20,0,20q�61   &   System.Windows.Controls.DataGrid  5   
   -   5          Name$ foldersDataGrid��5       $��1��6   
������    RowBackground �#1E1E1E   AlternatingRowBackground �#242424   RowHeaderWidth$ 0Q�5      $��0q�5         GridLinesVisibility$ 
Horizontal=�5         HorizontalGridLinesBrush �#333333   AutoGenerateColumns .     CanUserAddRows .  	   CanUserDeleteRows	 .  
   CanUserReorderColumns
 .     CanUserResizeRows .     CanUserSortColumns .    SelectionMode$ Single=�5%         HeadersVisibility$ Column=�5&      "   VerticalScrollBarVisibility$
 Auto=�5'      $   HorizontalScrollBarVisibility$
 Auto=�5(      $
��30,0q�5)       � 	Resources 5+      *  $System.Windows.Controls.DataGridCell �� 
TargetType �� Property �� Value �� 
TargetType=  7System.Windows.Controls.Primitives.DataGridColumnHeader��%�  '        '  �    �� 5-      "  6   .�5.      ��  � ��
 ���� 63   �� 5/       � ��$ 0q�68   �� 50       � ��$ 5q�60   �� 51       � ��$
 Bold"�63   �� 52       � ��$ 14(�61   �� 53       � �� 54      �� 55      "  6.   .R�56   "   �� #��z��6)   $��0q�6S   ._�57   &   �� $��Center=�67   $
��10,5q�6R   58   #   59      5;      5<      �� 5?      "  6   .�5@      ��  � ��
 ���� 63   �� 5A       � �� �#26853163   �� 5B       � ��$ 16(�61   �� 5C       � ��$
 Bold"�63   �� 5D       � ��$
 10,5q�60   �� 5E       � ��$ 0,0,0,1q�68   �� 5F       � �� �#33333364   �� 5G       � ��$
 Left=�6C   5H      5I         Columns 5K      0  *System.Windows.Controls.DataGridTextColumn  5M      ,  &System.Windows.Controls.DataGridColumn   Header$	 NOM��6%      Width5  /System.Windows.Controls.DataGridLengthConverter$	 140 6K   1  +System.Windows.Controls.DataGridBoundColumn   Binding 62   �� *Name+   ElementStyle 5N      �� 5O      " ��6    .�5P      ��  � ��
 �����6;   �� 5Q       � ��$ Center=�6B   �� 5R       � ��$
 Bold"�6;   �� 5S       � ��$ 14(�69   5T      5V        5Y      $ CHEMIN��6%   $ * 6T    65   �� *
FolderPath+ 5Z      �� 5[      " ��6    .�5\      ��  � ��
 �����6;   �� 5]       � ��$ Center=�6B   �� 5^       � ��$
 Bold"�6;   �� 5_       � ��$ 14(�69   �� 5`       � ��$ CharacterEllipsis=�6=   �� 5a       �@�ToolTip 68   �� *
FolderPath+5b      5d        5g      $ 
PROTECTION��6%   $	 140 6d    69   �� *ProtectionLevelDisplay+ 5h      �� 5i      " ��6    .�5j      ��  � ��
 �����6;   �� 5k       � ��$ Center=�6B   �� 5l       � ��$
 Bold"�6;   �� 5m       � ��$ 14(�69   5n      5p        5s      $ ÉTAT��6%   $	 150 6U    63   �� *StatusDisplay+ 5t      �� 5u      " ��6    .�5v      ��  � ��$ Center=�6B   �� 5w       � ��$
 Bold"�6;   �� 5x       � ��$ 14(�69   �� 5y       � �� 6;   �� *StatusColor+5z      5|      4  .System.Windows.Controls.DataGridTemplateColumn  5      $ 
DÉMARRAGE��6)   $	 120 6<      CellTemplate 5�      �� 5�      .R�5�      �� $��Center=�5�   '   $��Center=�6D   $��Activer��5�   '   
������� d� 	IsChecked 5�   '   �� *EnableOnStartup+ �� Mode$ TwoWay=� �� UpdateSourceTrigger$ PropertyChanged=�5�   9   5�      5�        5�      $ 80 6)    5�      �� 5�      .R�5�      ��  �� Orientation$ 
Horizontal=�6)   $��Center=�6B   .+�5�   "   �� -   5           IconButtonStyle#��[  5�   )     � ToolTip$  Modifier ce dossier��5�   )   $��30��5�   )   $��30��64   $	��2,0q�6@   $��0q�6M   
! � Tag! 5�   )   �� *Id+.��5�   &   F� �����@(��o � 6� �0F ׀�� PE  �� �� Q@KL ���(\+@W��C |�G�z-@ �� �� � 	= 0� ��uA 0�, �@KL  -1 �ЪM  -1��K� �`+�p=
ף�$@�" �� �� Ѱ)�Y� W@(]���(\@  L  L ����  �Q �\ gffff�1@Q��` ������-@� � @6� ��M�  i� � $� ��� �@?�  z �p � ��[ р��5� 	���#268531" �� Stretch$" Uniform=�5�   :   $��16��6L   $��16��6W   5�   #   �� 5�   "   -   5        #��[  5�   )   $��30��5�   )   $��30��64   $	��2,0q�6@   $��0q�6M   $  Supprimer ce dossier��5�   )   ! 5�   )   �� *Id+.��5�   &   F� �������!��j � $� ��j � $� ��[ ׀��  	=  �� �� р��  	= � z ��[  �� �� � z ��j �@KL ��j �@KL @T�  р�[ @T� р�[  -1�@T� ��^ ��- ��- ���� ��^׀� -1 ��- ��- р�@T� ���!@T� ���!��j  ���� ��[ р�� ��[ р�� ��j р�� ��j �@f -1� $� @o@�� z @o@���j  -1����j @T� �@f@T� ���� ����ا @f ��ا �ا �@T� �ا  �@T� @f׀�� �� ���� ������ @f ���� �ا �@]� �ا  �@]� @f׀�� �� 	���#db1f32$" Uniform=�5�   :   $��16��6L   $��16��6W   5�   #   5�      5�      5�      5�      � 5�   
   $��2��6   $��30,20q�6   # � ColumnDefinitions# 5�      �� 5�      $
��Auto �6#   �� 5�      $��* �6#   �� 5�      $
��Auto �6#   �� 5�      $
��Auto �6#   .+�5�      �� -   5        $��0��5�      $��Ajouter��6%     LaunchButtonStyle#��[ 5�      $	��120��5�      $��40��6!   $
��Left=�6-   ���#2685315�      �� 5�      -   5        $��1��5�      $��
Actualiser��6%   #��[ 5�      $	��120��5�      $��40��6!   $��10,0,0,0q�6-   $
��Left=�6?   5�      �� 5�      $��2��6   $ 
Horizontal=�6)   $��Center=�6B   $��Right=�6]   .+�5�      �� $ ��Verrouillage des dossiers:��6   
�������$��16(�6Q   $��Center=�6_   $��0,0,10,0q�6z   d� 5�      -   5        $ d�Name$$ toggleLockAll��5�      $
 TrueY�5�        ToggleSwitchStyle#��[ 5�      $��Center=�5�      $��0,0,20,0q�5�      5�      5�      5�      5�      