protoc.exe -I=./ --csharp_out=./ ./Protocol.proto 
IF ERRORLEVEL 1 PAUSE

START ../../../PacketGenerator2/bin/PacketGenerator2.exe ./Protocol.proto
XCOPY /Y Protocol.cs "E:\practice\20220621\Assets\Scrips\Packet"
XCOPY /Y Protocol.cs "E:\practice\20220710\Assets\Scrips\Packet"
XCOPY /Y Protocol.cs "../../../MMO_Server/Packet"
XCOPY /Y Protocol.cs "../../../DummyClient/Packet"
XCOPY /Y ClientPacketManager.cs "E:\practice\20220621\Assets\Scrips\Packet"
XCOPY /Y ClientPacketManager.cs "E:\practice\20220710\Assets\Scrips\Packet"
XCOPY /Y ClientPacketManager.cs "../../../DummyClient/Packet"
XCOPY /Y ServerPacketManager.cs "../../../MMO_Server/Packet"