   M S B A M L   `   `   `  ���� G  CCAY_Weighing, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:CAY_WeighingCAY_Weighing       CAY_Weighing.NumericKeypad   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     ,localclr-namespace:CAY_Weighing       � Title$  NumericKeypad��$	��313��$	��299�� � 
ResizeMode$ NoResize=� � WindowStartupLocation$ CenterScreen=� � Cursor$
 None�� � 	Resources 3  -System.Windows.Media.Effects.DropShadowEffect   DropShadowEffect   ShadowDepth   	Direction   
BlurRadius �� 
TargetType	 �� Property
 �� Value  IPhoneSteelBackground �� EndPoint �� 
StartPoint � Color � Offset  buttonBorder��%I  &	        '��      &	 B     &	 �     7   '�� �       $ 1Q�$	 -90Q�$ 5Q��� " ��.��� 	 � ��$
 22(��� $ 	0.5,1.272&�$ 0.5,0&�.��� $ 	#FF9097A3��$ 0Q�� $ 	#FF444D5B��$ 1Q��� " ��.��� 	 �@J�Effect8
   �� 	 � ��$
 4q��� " ��.��� 	 � ��$
 4q��� 	 � ��$
 24(�.��� #���   � RowDefinitions �� �� �� �� ��  � ColumnDefinitions �� �� �� �� .+�� $��1��.+��� #��[ 
���   � �� CornerRadius$ 4���� -   $��button5��
�������$��1q�$��NUMPAD7��.��7� $��1��$��1��.+��� #��[ 
���   �$ 4���� -    ��Name$ button6��
�������$��NUMPAD8��$��1q�$��8��� $��2��$��1��.+��� #��[ 
���   �$ 4���� -   $ button7��
�������$��NUMPAD9��$��9��� $��3��$��1��.+��� #��[ 
���   �$ 4���� -   $ button8��$
��BACK��.��F�  �� Stretch$
 Fill=�$��36��$��24�������1!�_\ќ�� ��~�4� ��S�|�� Lz~��} ĩ^��J� #4Ѵ1| ,�Ć� ���ԟ� ��Q�v� �@��ǭp@�dC!|��Dt� l�1P�Y!��:�p@Q\"z ��:�p@Sln_ ��:�p@$t�\R#$t��5:�$t�Qln_ ���\"z ֨��Y!֨���O!֨�bW!��bW!�kQ�bW!��:�p@S�bW!2U0*��p@��M!��:�p@�Y!��:�p@���	#FF333333� $��2��.+��� #��[ 
���   �$ 4���� -   $ button9��
�������$��NUMPAD4��$��4��� $��1��$��2��.+��� #��[ 
���   �$ 4���� -   $ button10��
�������$��NUMPAD5��$��5��� $��2��$��2��.+��� #��[ 
���   �$ 4���� -   $ button11��
�������$��NUMPAD6��$��6��� $��3��.+��� #��[ 
���   �$ 4���� -   $ button13��
�������$��NUMPAD1��$��1��� $��1��$��3��.+��� #��[ 
���   �$ 4���� -	   $ button14��
�������$��NUMPAD2��$��2��� $��2��$��3��.+��� #��[ 
���   �$ 4���� -
   $ button15��
�������$��NUMPAD3��$��3��� $��3��$��2��$��3��.+��� #��[ 
���   �$ 4���� -   $ button16��$��RETURN��$��Enter��� $��2��$��4��.+��� #��[ 
���   �$ 4���� -   $ button17��
�������$��NUMPAD0��$��0��� $��2��$��4��.+��� #��[ 
���   �$ 4���� -   $ btnDot��
�������$��DECIMAL��$��.��� $��3��.+��� #��[ 
���   �$ 0���� $��Right=�
�������$��4q����� *Result+� $��3��.+��� #��[ 
���   �$ 4���� -   $ 	buttonEsc��$��0��$	��ESC��$	��Esc��