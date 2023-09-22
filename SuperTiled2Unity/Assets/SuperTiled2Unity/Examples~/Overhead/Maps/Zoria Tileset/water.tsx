<?xml version="1.0" encoding="UTF-8"?>
<tileset name="water" tilewidth="16" tileheight="16" tilecount="228" columns="12">
 <properties>
  <property name="unity:isTrigger" type="bool" value="true"/>
  <property name="unity:layer" value="Water"/>
 </properties>
 <image source="water.png" width="192" height="304"/>
 <tile id="0">
  <objectgroup draworder="index">
   <object id="1" x="0" y="0" width="16" height="16"/>
  </objectgroup>
 </tile>
 <tile id="24">
  <objectgroup draworder="index">
   <object id="1" x="0" y="0" width="16" height="16"/>
   <object id="8" x="0" y="0" width="4" height="16">
    <properties>
     <property name="unity:isTrigger" type="bool" value="false"/>
     <property name="unity:layer" value="Default"/>
    </properties>
   </object>
   <object id="9" x="0" y="0" width="16" height="4">
    <properties>
     <property name="unity:isTrigger" type="bool" value="false"/>
     <property name="unity:layer" value="Default"/>
    </properties>
   </object>
  </objectgroup>
  <animation>
   <frame tileid="24" duration="250"/>
   <frame tileid="27" duration="250"/>
   <frame tileid="30" duration="250"/>
   <frame tileid="33" duration="250"/>
  </animation>
 </tile>
 <tile id="25">
  <objectgroup draworder="index">
   <object id="1" x="0" y="0" width="16" height="16"/>
   <object id="2" x="0" y="0" width="16" height="4">
    <properties>
     <property name="unity:isTrigger" type="bool" value="false"/>
     <property name="unity:layer" value="Default"/>
    </properties>
   </object>
  </objectgroup>
  <animation>
   <frame tileid="25" duration="250"/>
   <frame tileid="28" duration="250"/>
   <frame tileid="31" duration="250"/>
   <frame tileid="34" duration="250"/>
  </animation>
 </tile>
 <tile id="36">
  <objectgroup draworder="index">
   <object id="1" x="0" y="0" width="16" height="16"/>
   <object id="2" x="0" y="0" width="4" height="16">
    <properties>
     <property name="unity:isTrigger" type="bool" value="false"/>
     <property name="unity:layer" value="Default"/>
    </properties>
   </object>
  </objectgroup>
  <animation>
   <frame tileid="36" duration="250"/>
   <frame tileid="39" duration="250"/>
   <frame tileid="42" duration="250"/>
   <frame tileid="45" duration="250"/>
  </animation>
 </tile>
 <tile id="37">
  <objectgroup draworder="index">
   <object id="1" x="0" y="0" width="16" height="16"/>
  </objectgroup>
  <animation>
   <frame tileid="37" duration="250"/>
   <frame tileid="40" duration="250"/>
   <frame tileid="43" duration="250"/>
   <frame tileid="46" duration="250"/>
  </animation>
 </tile>
 <tile id="48">
  <objectgroup draworder="index">
   <object id="1" x="0" y="0" width="16" height="16"/>
   <object id="2" x="0" y="0" width="4" height="16">
    <properties>
     <property name="unity:isTrigger" type="bool" value="false"/>
     <property name="unity:layer" value="Default"/>
    </properties>
   </object>
   <object id="3" x="0" y="12" width="16" height="4">
    <properties>
     <property name="unity:isTrigger" type="bool" value="false"/>
     <property name="unity:layer" value="Default"/>
    </properties>
   </object>
  </objectgroup>
  <animation>
   <frame tileid="48" duration="250"/>
   <frame tileid="51" duration="250"/>
   <frame tileid="54" duration="250"/>
   <frame tileid="57" duration="250"/>
  </animation>
 </tile>
 <tile id="49">
  <objectgroup draworder="index">
   <object id="1" x="0" y="0" width="16" height="16"/>
   <object id="2" x="0" y="12" width="16" height="4">
    <properties>
     <property name="unity:isTrigger" type="bool" value="false"/>
     <property name="unity:layer" value="Default"/>
    </properties>
   </object>
  </objectgroup>
  <animation>
   <frame tileid="49" duration="250"/>
   <frame tileid="52" duration="250"/>
   <frame tileid="55" duration="250"/>
   <frame tileid="58" duration="250"/>
  </animation>
 </tile>
 <tile id="121">
  <objectgroup draworder="index">
   <object id="1" x="0" y="0" width="16" height="16"/>
  </objectgroup>
  <animation>
   <frame tileid="121" duration="250"/>
   <frame tileid="124" duration="250"/>
   <frame tileid="127" duration="250"/>
   <frame tileid="130" duration="250"/>
  </animation>
 </tile>
 <tile id="122">
  <objectgroup draworder="index">
   <object id="1" x="0" y="0" width="16" height="16"/>
  </objectgroup>
  <animation>
   <frame tileid="122" duration="250"/>
   <frame tileid="125" duration="250"/>
   <frame tileid="128" duration="250"/>
   <frame tileid="131" duration="250"/>
  </animation>
 </tile>
 <tile id="133">
  <objectgroup draworder="index">
   <object id="1" x="0" y="0" width="16" height="16"/>
  </objectgroup>
  <animation>
   <frame tileid="133" duration="250"/>
   <frame tileid="136" duration="250"/>
   <frame tileid="139" duration="250"/>
   <frame tileid="142" duration="250"/>
  </animation>
 </tile>
 <tile id="134">
  <objectgroup draworder="index">
   <object id="1" x="0" y="0" width="16" height="16"/>
  </objectgroup>
  <animation>
   <frame tileid="134" duration="250"/>
   <frame tileid="137" duration="250"/>
   <frame tileid="140" duration="250"/>
   <frame tileid="143" duration="250"/>
  </animation>
 </tile>
 <tile id="192">
  <properties>
   <property name="unity:isTrigger" type="bool" value="false"/>
   <property name="unity:layer" value="Default"/>
  </properties>
  <objectgroup draworder="index">
   <object id="1" x="0" y="0" width="16" height="16"/>
  </objectgroup>
  <animation>
   <frame tileid="192" duration="250"/>
   <frame tileid="193" duration="250"/>
   <frame tileid="194" duration="250"/>
   <frame tileid="195" duration="250"/>
  </animation>
 </tile>
 <tile id="204">
  <properties>
   <property name="unity:isTrigger" type="bool" value="false"/>
   <property name="unity:layer" value="Default"/>
  </properties>
  <objectgroup draworder="index">
   <object id="1" x="0" y="0" width="16" height="16"/>
  </objectgroup>
  <animation>
   <frame tileid="204" duration="250"/>
   <frame tileid="205" duration="250"/>
   <frame tileid="206" duration="250"/>
   <frame tileid="207" duration="250"/>
  </animation>
 </tile>
</tileset>
