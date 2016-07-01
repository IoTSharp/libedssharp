﻿/*
    This file is part of libEDSsharp.

    libEDSsharp is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Foobar is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with libEDSsharp.  If not, see <http://www.gnu.org/licenses/>.
 
    Copyright(c) 2016 Robin Cornelius <robin.cornelius@gmail.com>
    based heavly on the files CO_OD.h and CO_OD.c from CanOpenNode which are
    Copyright(c) 2010 - 2016 Janez Paternoster
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

namespace libEDSsharp
{
    public class CanOpenNodeExporter
    {

        private string folderpath;
        private EDSsharp eds;

    //    Dictionary<DataType, defstruct> defstructs = new Dictionary<DataType, defstruct>();

        public void export(string folderpath, EDSsharp eds)
        {
            this.folderpath = folderpath;
            this.eds = eds;

            //init_defstructs();

            countPDOS();

            export_h();
            export_c();

        }

        private void print_h_bylocation(StreamWriter file, StorageLocation location)
        {
            foreach (KeyValuePair<UInt16, ODentry> kvp in eds.ods)
            {
                ODentry od = kvp.Value;


                if ((od.location != location))
                {
                    if (!(od.location == 0 && location == StorageLocation.RAM))
                        continue;
                }


                if (od.nosubindexes == 0)
                {
                    //if (od.subindex == -1)
                    {

                        file.WriteLine(string.Format("/*{0:x4}      */ {1,-15} {2};", od.index, od.datatype.ToString(), od.paramater_cname()));
                    }
                }
                else
                {
                    DataType t = eds.getdatatype(od);
                    file.WriteLine(string.Format("/*{0:x4}      */ {1,-15} {2}[{3}];", od.index, t.ToString(), od.paramater_cname(), od.nosubindexes));
                }

            }
        }

        private void addGPLheader(StreamWriter file)
        {
            file.WriteLine(@"/*******************************************************************************

   File - CO_OD.c/CO_OD.h
   CANopen Object Dictionary.

   Copyright (C) 2004-2008 Janez Paternoster

   License: GNU Lesser General Public License (LGPL).

   <http://canopennode.sourceforge.net>

   (For more information see <CO_SDO.h>.)
*/
/*
   This program is free software: you can redistribute it and/or modify
   it under the terms of the GNU Lesser General Public License as published by
   the Free Software Foundation, either version 3 of the License, or
   (at your option) any later version.

   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU Lesser General Public License for more details.

   You should have received a copy of the GNU Lesser General Public License
   along with this program.  If not, see <http://www.gnu.org/licenses/>.


   Original Author: Janez Paternoster


   This file was automatically generated with libedssharp Object
   Dictionary Editor. DON'T EDIT THIS FILE MANUALLY !!!!

*******************************************************************************/

");

        }

        private void export_h()
        {

            StreamWriter file = new StreamWriter(folderpath + Path.DirectorySeparatorChar + "CO_OD.h");


            addGPLheader(file);

            file.WriteLine("#pramga once");
            file.WriteLine("");

            file.WriteLine(@"/*******************************************************************************
   CANopen DATA DYPES
*******************************************************************************/
   typedef uint8_t      UNSIGNED8;
   typedef uint16_t     UNSIGNED16;
   typedef uint32_t     UNSIGNED32;
   typedef uint64_t     UNSIGNED64;
   typedef int8_t       INTEGER8;
   typedef int16_t      INTEGER16;
   typedef int32_t      INTEGER32;
   typedef int64_t      INTEGER64;
   typedef float32_t    REAL32;
   typedef float64_t    REAL64;
   typedef char_t       VISIBLE_STRING;
   typedef oChar_t      OCTET_STRING;
   typedef domain_t     DOMAIN;

");

            file.WriteLine("/*******************************************************************************");
            file.WriteLine("   FILE INFO:");
            file.WriteLine(string.Format("      FileName:     {0}", eds.fi.FileName));
            file.WriteLine(string.Format("      FileVersion:  {0}", eds.fi.FileVersion));
            file.WriteLine(string.Format("      CreationTime: {0}", eds.fi.CreationTime));
            file.WriteLine(string.Format("      CreationDate: {0}", eds.fi.CreationDate));
            file.WriteLine(string.Format("      CreatedBy:    {0}", eds.fi.CreatedBy));
            file.WriteLine("/******************************************************************************/");
            file.WriteLine("");
            file.WriteLine("");

            file.WriteLine("/*******************************************************************************");
            file.WriteLine("   DEVICE INFO:");
            file.WriteLine(string.Format("      VendorName:     {0}", eds.di.VendorName));
            file.WriteLine(string.Format("      VendorNumber    {0}", eds.di.VendorNumber));
            file.WriteLine(string.Format("      ProductName:    {0}", eds.di.ProductName));
            file.WriteLine(string.Format("      ProductNumber:  {0}", eds.di.ProductNumber));
            file.WriteLine("/******************************************************************************/");
            file.WriteLine("");
            file.WriteLine("");

         
            file.WriteLine(@"/*******************************************************************************
   FEATURES
*******************************************************************************/");

            file.WriteLine(string.Format("  #define CO_NO_SYNC                     {0}   //Associated objects: 1005-1007", noSYNC));

            file.WriteLine(string.Format("  #define CO_NO_EMERGENCY                {0}   //Associated objects: 1014, 1015", noEMCY));

            file.WriteLine(string.Format("  #define CO_NO_SDO_SERVER               {0}   //Associated objects: 1200-127F", noSDOservers));
            file.WriteLine(string.Format("  #define CO_NO_SDO_CLIENT               {0}   //Associated objects: 1280-12FF", noSDOclients));

            file.WriteLine(string.Format("  #define CO_NO_RPDO                     {0}   //Associated objects: 14xx, 16xx", noRXpdos));
            file.WriteLine(string.Format("  #define CO_NO_TPDO                     {0}   //Associated objects: 18xx, 1Axx", noTXpdos));

            //FIX ME NMT MASTER should auto generate
            file.WriteLine(@"  #define CO_NO_NMT_MASTER               0   

");

            file.WriteLine(@"/*******************************************************************************
   OBJECT DICTIONARY
*******************************************************************************/");
            file.WriteLine(string.Format("   #define CO_OD_NoOfElements             {0}", eds.ods.Count));
            file.WriteLine("");
            file.WriteLine("");

            file.WriteLine(@"/*******************************************************************************
   TYPE DEFINITIONS FOR RECORDS
*******************************************************************************/");

            //We need to identify all the record types used and generate a struct for each one
            //FIXME the original CanOpenNode exporter said how many items used this struct in the comments

            List<string> structnamelist = new List<string>();

            foreach (KeyValuePair<UInt16, ODentry> kvp in eds.ods)
            {
                ODentry od = kvp.Value;

                if (od.objecttype != ObjectType.REC)
                    continue;

                string structname = String.Format("OD_{0}_t", make_cname(od.parameter_name));

                if (structnamelist.Contains(structname))
                    continue;

                structnamelist.Add(structname);

                file.WriteLine(string.Format("/*{0:x4}    */ typedef struct {{", kvp.Key));
                foreach (KeyValuePair<UInt16, ODentry> kvp2 in kvp.Value.subobjects)
                {
                    ODentry subod = kvp2.Value;
                    file.WriteLine(string.Format("               {0,-15}{1};", subod.datatype.ToString(), make_cname(subod.parameter_name)));

                }

                file.WriteLine(string.Format("               }}              {0};", structname));

            }

    

            file.WriteLine(@"/*******************************************************************************
   STRUCTURES FOR VARIABLES IN DIFFERENT MEMORY LOCATIONS
*******************************************************************************/
#define  CO_OD_FIRST_LAST_WORD     0x55 //Any value from 0x01 to 0xFE. If changed, EEPROM will be reinitialized.

/***** Structure for RAM variables ********************************************/
struct sCO_OD_RAM{
               UNSIGNED32     FirstWord;
");

            print_h_bylocation(file, StorageLocation.RAM);

            file.WriteLine(@"
               UNSIGNED32     LastWord;
};");

            file.WriteLine(@"/***** Structure for EEPROM variables *****************************************/
struct sCO_OD_EEPROM{
               UNSIGNED32     FirstWord;


");
            print_h_bylocation(file, StorageLocation.EEPROM);

            file.WriteLine(@"
               UNSIGNED32     LastWord;
};");

            file.WriteLine(@"/***** Structure for ROM variables ********************************************/
struct sCO_OD_ROM{
               UNSIGNED32     FirstWord;


");
            print_h_bylocation(file, StorageLocation.ROM);

            file.WriteLine(@"
               UNSIGNED32     LastWord;
};");


            file.WriteLine(@"/***** Declaration of Object Dictionary variables *****************************/
extern struct sCO_OD_RAM CO_OD_RAM;

extern struct sCO_OD_EEPROM CO_OD_EEPROM;

extern CO_OD_ROM_IDENT struct sCO_OD_ROM CO_OD_ROM;


/*******************************************************************************
   ALIASES FOR OBJECT DICTIONARY VARIABLES
*******************************************************************************/");

            foreach (KeyValuePair<UInt16, ODentry> kvp in eds.ods)
            {


                ODentry od = kvp.Value;

                string loc = getlocation(od.location);

                DataType t = eds.getdatatype(od);

                if (od.nosubindexes == 0)
                {
                    file.WriteLine(string.Format("/*{0:x4}, Data Type: {1} */", od.index, t.ToString()));
                    file.WriteLine(string.Format("        #define {0,-51}{1}.{2}", string.Format("OD_{0}",od.paramater_cname()), loc, od.paramater_cname()));
                }
                else
                {

                    //ARRAY TYPES SUBS ONLY FIXME

                    DataType dt = od.datatype;

                    file.WriteLine(string.Format("/*{0:x4}, Data Type: {1}, Array[{2}] */", od.index, t.ToString(), od.nosubindexes - 1));
                    file.WriteLine(string.Format("        #define OD_{0,-48}{1}.{2}", od.paramater_cname(), loc, od.paramater_cname()));
                    file.WriteLine(string.Format("        #define {0,-51}{1}", string.Format("ODL_{0}_arrayLength", od.paramater_cname()), od.nosubindexes - 1));


                    if (od.objecttype != ObjectType.ARRAY)
                    {
                        List<string> ODAs = new List<string>();

                        string ODAout = "";

                        foreach (KeyValuePair<UInt16, ODentry> kvp2 in od.subobjects)
                        {
                            ODentry sub = kvp2.Value;

                            if (sub.subindex == 0)
                                continue;

                            string ODA = string.Format("{0}", string.Format("ODA_{0}_{1}", od.paramater_cname(), sub.paramater_cname()));

                            if (ODAs.Contains(ODA))
                            {
                                ODAout = "";
                                break;
                            }

                            ODAs.Add(ODA);


                            ODAout += (string.Format("        #define {0,-51}{1}\r\n", string.Format("ODA_{0}_{1}", od.paramater_cname(), sub.paramater_cname()), sub.subindex));

                        }

                        file.Write(ODAout);
                    }
                }

                file.WriteLine("");

            }

            file.Close();

        }

        private void export_c()
        {
            StreamWriter file = new StreamWriter(folderpath + Path.DirectorySeparatorChar + "CO_OD.c");

            addGPLheader(file);

            file.WriteLine(@"#include ""CO_driver.h""
#include ""CO_OD.h""
#include ""CO_SDO.h""


/*******************************************************************************
   DEFINITION AND INITIALIZATION OF OBJECT DICTIONARY VARIABLES
*******************************************************************************/

/***** Definition for RAM variables *******************************************/
struct sCO_OD_RAM CO_OD_RAM = {
           CO_OD_FIRST_LAST_WORD,
");

            export_OD_def_array(file, StorageLocation.RAM);

            file.WriteLine(@"
           CO_OD_FIRST_LAST_WORD,
};

/***** Definition for EEPROM variables ****************************************/
struct sCO_OD_EEPROM CO_OD_EEPROM = {
           CO_OD_FIRST_LAST_WORD,
");


            export_OD_def_array(file, StorageLocation.EEPROM);

            file.WriteLine(@"  CO_OD_FIRST_LAST_WORD,
};


/***** Definition for ROM variables *******************************************/
   CO_OD_ROM_IDENT struct sCO_OD_ROM CO_OD_ROM = {    //constant variables, stored in flash
           CO_OD_FIRST_LAST_WORD,

");


            export_OD_def_array(file, StorageLocation.ROM);

            file.WriteLine(@"

           CO_OD_FIRST_LAST_WORD
};


/*******************************************************************************
   STRUCTURES FOR RECORD TYPE OBJECTS
*******************************************************************************/

");

            export_record_types(file);


            file.Write(@"/*******************************************************************************
   OBJECT DICTIONARY
*******************************************************************************/
const sCO_OD_object CO_OD[");
            
            file.Write(string.Format("{0}",eds.ods.Count));

            file.WriteLine(@"] = {
");

            foreach (KeyValuePair<UInt16, ODentry> kvp in eds.ods)
            {

                ODentry od = kvp.Value;

                string loc = getlocation(od.location);

                byte flags = getflags(od);

                DataType t = eds.getdatatype(od);
                int datasize = od.sizeofdatatype();

                string odf;

                if (od.AccessFunctionName != null)
                {
                    odf = od.AccessFunctionName;
                }
                else
                {
                    odf = "CO_ODF";
                }

                string array = "";
                if (od.nosubindexes > 0)
                    array = string.Format("[0]");

                file.WriteLine(string.Format("{{0x{0:x4}, 0x{1:x2}, 0x{2:x2}, {3}, (const void*)&{4}.{5}{6}}},", od.index, od.nosubindexes, flags, datasize, loc, od.paramater_cname(), array));

            }



            file.WriteLine("};");

            file.Close();
        }



        byte getflags(ODentry od)
        {
            byte flags = 0;

            flags = (byte)od.location;

            //fixme rwr and rrw are not supported
            if (od.accesstype == EDSsharp.AccessType.ro
                || od.accesstype == EDSsharp.AccessType.rw
                || od.accesstype == EDSsharp.AccessType.cons)
                flags |= 0x04;

            if (od.accesstype == EDSsharp.AccessType.wo
                || od.accesstype == EDSsharp.AccessType.rw)
                flags |= 0x08;

            if (od.PDOMapping)
                flags |= 0x10;

            if (od.PDOMapping)
                flags |= 0x20;

            if (od.PDOMapping)
                flags |= 0x30; //fix me no control over rx and tx mapping, its both or none

            if(od.TPDODetectCos)
              flags |=0x40;
   
            int datasize = od.sizeofdatatype();

            if (datasize > 1)
                flags |= 0x80;

            return flags;

        }


        string getlocation(StorageLocation location)
        {
            string loc;
            switch (location)
            {
                case StorageLocation.ROM:
                    loc = "CO_OD_ROM";
                    break;
                default:
                case StorageLocation.RAM:
                    loc = "CO_OD_RAM";
                    break;
                case StorageLocation.EEPROM:
                    loc = "CO_OD_EEPROM";
                    break;

            }

            return loc;
        }

        string formatvaluewithdatatype(string defaultvalue, DataType dt)
        {
            int nobase = 10;
            bool nodeidreplace = false;

            if (defaultvalue.Contains("$NODEID"))
            {
                defaultvalue = defaultvalue.Replace("$NODEID", "");
                defaultvalue = defaultvalue.Replace("+", "");
                nodeidreplace = true;
            }

            String pat = @"^0[xX][0-9]+";

            Regex r = new Regex(pat, RegexOptions.IgnoreCase);
            Match m = r.Match(defaultvalue);
            if (m.Success)
            {
                nobase = 16;
            }

            pat = @"^0[0-9]+";
            r = new Regex(pat, RegexOptions.IgnoreCase);
            m = r.Match(defaultvalue);
            if (m.Success)
            {
                nobase = 8;
            }

            if (nodeidreplace)
            {
                UInt16 data = Convert.ToUInt16(defaultvalue, nobase);
                data += eds.NodeId;
                defaultvalue = string.Format("0x{0:x}", data);
                nobase = 16;
            }


            switch (dt)
            {
                case DataType.UNSIGNED24:
                case DataType.UNSIGNED32:
                    return String.Format("0x{0:x4}L", Convert.ToUInt32(defaultvalue, nobase));

                case DataType.INTEGER24:
                case DataType.INTEGER32:
                    return String.Format("0x{0:x4}L", Convert.ToInt32(defaultvalue, nobase));

                case DataType.REAL32:
                case DataType.REAL64:
                    return (String.Format("{0}", defaultvalue));


                //fix me this looks wrong
                case DataType.UNICODE_STRING:
                    return (String.Format("'{0}'", defaultvalue));

                case DataType.VISIBLE_STRING:
                    {
                        string array = "{";
                        foreach (char s in defaultvalue)
                        {
                            array += "'" + s + "'";

                            if (!object.ReferenceEquals(s, defaultvalue.Last()))
                            {
                                array += ", ";
                            }
                        }

                        array += "}";
                        return array;
                    }


                case DataType.OCTET_STRING:
                    {
                        string[] bits = defaultvalue.Split(' ');
                        string octet = "";
                        foreach (string s in bits)
                        {
                            octet += formatvaluewithdatatype(s, DataType.UNSIGNED8);

                            if (!object.ReferenceEquals(s, bits.Last()))
                            {
                                octet += ", ";
                            }
                        }
                        return octet;
                    }

                case DataType.INTEGER8:
                    return String.Format("0x{0:x1}", Convert.ToSByte(defaultvalue, nobase));

                case DataType.INTEGER16:
                    return String.Format("0x{0:x2}", Convert.ToInt16(defaultvalue, nobase));

                case DataType.UNSIGNED8:
                    return String.Format("0x{0:x1}L", Convert.ToByte(defaultvalue, nobase));

                case DataType.UNSIGNED16:
                    return String.Format("0x{0:x2}", Convert.ToUInt16(defaultvalue, nobase));

                default:
                    return (String.Format("{0:x}", defaultvalue));

            }
        }

       string make_cname(string name)
       {
           string[] bits = Regex.Split(name,@"[\W]+");

           string output = "";

           char lastchar = ' ';
           foreach (string s in bits)
           {
               if(Char.IsUpper(lastchar) && Char.IsUpper(s.First()))
                    output+="_";

               output +=char.ToUpper(s[0]) + s.Substring(1);

               lastchar = output.Last();
           }

           return output;
       }

        void export_record_types(StreamWriter file)
        {


            foreach (KeyValuePair<UInt16, ODentry> kvp in eds.ods)
            {


                ODentry od = kvp.Value;

                if (od.objecttype != ObjectType.REC)
                    continue;

                if (od.datatype == DataType.UNKNOWN)
                    continue;

                int count = od.subobjects.Count; //don't include index

                if(od.index>=0x1400 && od.index<0x1600)
                {
                    count = 3; //CanOpenNode Fudging. Its only 3 paramaters for RX PDOS in the c code despite being a PDO_COMMUNICATION_PARAMETER
                }

                string cname = make_cname(od.parameter_name);
                
                file.WriteLine(String.Format("/*0x{0:x4}*/ const CO_OD_entryRecord_t OD_record{0:x4}[{1}] = {{", od.index, count));

                foreach (KeyValuePair<UInt16, ODentry> kvpsub in od.subobjects)
                {
                    ODentry sub = kvpsub.Value;

                    string subcname = make_cname(sub.parameter_name);

                    file.WriteLine(string.Format("           {{(void*)&CO_OD_ROM.OD_{0}_t.{1}, 0x{2:x2}, 0x{3} }}", cname, subcname, getflags(sub), sub.sizeofdatatype()));

                }

                file.Write("};\r\n\r\n");
            }
        }

        int noTXpdos = 0;
        int noRXpdos = 0;
        int noSDOclients = 0;
        int noSDOservers = 0;
        int noSYNC = 0;
        int noEMCY = 0;

        void countPDOS()
        {
            noRXpdos = 0;
            noTXpdos = 0;

            foreach (KeyValuePair<UInt16, ODentry> kvp in eds.ods)
            {
                UInt16 index = kvp.Key;

                if ((index & 0xFF00) == 0x1400)
                {
                    noRXpdos++;
                }

                if ((index & 0xFF00) == 0x1800)
                {
                    noTXpdos++;
                }

                if((index & 0xFF80) == 0x1200)
                {
                    noSDOservers++;
                }

                if ((index & 0xFF80) == 0x1280)
                {
                    noSDOclients++;
                }

                if (index == 0x1005)
                    noSYNC = 1;

                if (index == 0x1014)
                    noEMCY = 1;
            }

        }

        bool arrayspecial(UInt16 index, bool open)
        {

            if (open)
            {
                if (index == 0x1200)
                    return true;

                if (index == 0x1400)
                    return true;

                if (index == 0x1600)
                    return true;

                if (index == 0x1800)
                    return true;

                if (index == 0x1a00)
                    return true;
            }
            else
            {
                if (index == 0x1200)
                    return true;

                if (index == 0x1400 + noRXpdos - 1)
                    return true;

                if (index == 0x1600 + noRXpdos - 1)
                    return true;

                if (index == 0x1800 + noTXpdos - 1)
                    return true;

                if (index == 0x1a00 + noTXpdos - 1)
                    return true;
            }

            return false;

        }


        void export_OD_def_array(StreamWriter file, StorageLocation location)
        {

            foreach (KeyValuePair<UInt16, ODentry> kvp in eds.ods)
            {
                ODentry od = kvp.Value;

                if ((od.location != location))
                {
                    if (!(od.location == 0 && location == StorageLocation.RAM))
                        continue;
                }

                if (od.nosubindexes == 0)
                {
                    file.WriteLine(string.Format("/*{0:x4}*/ {1},", od.index, formatvaluewithdatatype(od.defaultvalue, od.datatype)));
                }
                else
                {
                    if (arrayspecial(od.index, true))
                    {
                        file.Write(string.Format("/*{0:x4}*/ {{{{", od.index));
                    }
                    else
                    {
                        file.Write(string.Format("/*{0:x4}*/ {{", od.index));
                    }

                    foreach (KeyValuePair<UInt16, ODentry> kvp2 in od.subobjects)
                    {
                        ODentry sub = kvp2.Value;

                        DataType dt = sub.datatype;

                        if ((od.objecttype == ObjectType.REC ||od.objecttype==ObjectType.ARRAY) && sub.subindex == 0)
                            continue;

                        if (od.objecttype == ObjectType.REC)
                            dt = od.datatype;

                        file.Write(formatvaluewithdatatype(sub.defaultvalue, dt));

                        if (od.subobjects.Keys.Last() != kvp2.Key)
                            file.Write(", ");
                    }


                    if (arrayspecial(od.index, false))
                    {
                        file.WriteLine("}},");
                    }
                    else
                    {
                        file.WriteLine("},");
                    }

                }
            }

        }

    }
}