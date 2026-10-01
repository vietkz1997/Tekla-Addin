template _tmp_843
{
    name = "template1";
    type = GRAPHICAL;
    width = 142;
    maxheight = 120;
    columns = (1, 1);
    gap = 5;
    fillpolicy = EVEN;
    filldirection = HORIZONTAL;
    fillstartfrom = TOPLEFT;
    margins = (0, 0, 0, 0);
    gridxspacing = 0.2;
    gridyspacing = 0.2;
    version = 3.6;
    created = "26.12.2008 14:33";
    modified = "18.07.2018 09:29";
    notes = "";

    header _tmp_1135
    {
        name = "Header";
        height = 5;

        text _tmp_1177
        {
            name = "H594X302X14X23_2";
            x1 = 30.7030321083824;
            y1 = 0.896086275294233;
            x2 = 30.7030321083824;
            y2 = 0.896086275294233;
            string = "FIELD BOLTS";
            fontname = "romsim";
            fontcolor = 153;
            fonttype = 4;
            fontsize = 3;
            fontratio = 0.8;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        text _tmp_1200
        {
            name = "Text_2";
            x1 = 64;
            y1 = 0.999999999999993;
            x2 = 64;
            y2 = 0.999999999999993;
            string = "GRADE";
            fontname = "romsim";
            fontcolor = 153;
            fonttype = 4;
            fontsize = 3;
            fontratio = 0.8;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = CENTERED;
            pen = -1;
        };

        group _tmp_972
        {
            name = "Group_1";

            lineorarc _tmp_973
            {
                name = "LineOrArc_16";
                x1 = 118.994355598446;
                y1 = 0;
                x2 = 118.994355598446;
                y2 = 5;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_974
            {
                name = "LineOrArc_16";
                x1 = 0;
                y1 = 0;
                x2 = 142;
                y2 = 0;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_975
            {
                name = "LineOrArc_16";
                x1 = 0;
                y1 = 5;
                x2 = 142;
                y2 = 5;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_976
            {
                name = "LineOrArc_16";
                x1 = 101.617070530028;
                y1 = 0;
                x2 = 101.617070530028;
                y2 = 5;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_977
            {
                name = "LineOrArc_16";
                x1 = 79.4397964385726;
                y1 = 0;
                x2 = 79.4397964385726;
                y2 = 5;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_978
            {
                name = "LineOrArc_16";
                x1 = 62.3885297500254;
                y1 = 0;
                x2 = 62.3885297500254;
                y2 = 5;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_979
            {
                name = "LineOrArc_16";
                x1 = 22.1726125811006;
                y1 = 0;
                x2 = 22.1726125811006;
                y2 = 5;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_980
            {
                name = "LineOrArc_16";
                x1 = 0;
                y1 = 0;
                x2 = 0;
                y2 = 5;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };
        };
    };

    row _tmp_870
    {
        name = "ASSEMBLY";
        height = 1;
        visibility = FALSE;
        usecolumns = FALSE;
        rule = "";
        contenttype = "ASSEMBLY";
        sorttype = COMBINE;

        row _tmp_6438
        {
            name = "SIMILAR_ASSEMBLY";
            height = 1;
            visibility = FALSE;
            usecolumns = FALSE;
            rule = "";
            contenttype = "SIMILAR_ASSEMBLY";
            sorttype = COMBINE;

            row _tmp_897
            {
                name = "PART";
                height = 1;
                visibility = FALSE;
                usecolumns = FALSE;
                rule = "";
                contenttype = "PART";
                sorttype = COMBINE;

                row _tmp_1296
                {
                    name = "BOLT";
                    height = 5;
                    visibility = TRUE;
                    usecolumns = FALSE;
                    rule = "";
                    contenttype = "BOLT";
                    sorttype = COMBINE;

                    valuefield _tmp_2019
                    {
                        name = "ValueField_3";
                        location = (128.49853515625, 0.79999999999999);
                        formula = "GetValue(\"NUMBER\")";
                        maxnumoflines = 1;
                        datatype = INTEGER;
                        class = "";
                        cacheable = TRUE;
                        formatzeroasempty = FALSE;
                        justify = RIGHT;
                        visibility = FALSE;
                        angle = 0;
                        length = 5;
                        sortdirection = NONE;
                        fontname = "Arial";
                        fontcolor = 161;
                        fonttype = 2;
                        fontsize = 3;
                        fontratio = 1;
                        fontstyle = 0;
                        fontslant = 0;
                        pen = -1;
                        oncombine = SUM;
                    };

                    valuefield _tmp_2313
                    {
                        name = "length";
                        location = (104.49755859375, 0.79999999999999);
                        formula = "GetValue(\"LENGTH\")";
                        datatype = DOUBLE;
                        class = "Length";
                        cacheable = TRUE;
                        justify = RIGHT;
                        visibility = FALSE;
                        angle = 0;
                        length = 5;
                        decimals = 0;
                        sortdirection = ASCENDING;
                        fontname = "Arial";
                        fontcolor = 161;
                        fonttype = 2;
                        fontsize = 3;
                        fontratio = 1;
                        fontstyle = 0;
                        fontslant = 0;
                        pen = -1;
                        oncombine = NONE;
                        unit = "mm";
                    };

                    valuefield _tmp_1755
                    {
                        name = "FIELD_BOLT";
                        location = (22.5, 0.79999999999999);
                        formula = "Sum(\"ValueField_3\")+\" - M\"+int(GetFieldFormula(\"DIA\"))+\" \"+GetFieldFormula(\"type\")+\" \"+\"X\"+\" \"+int(GetFieldFormula(\"length\"))";
                        datatype = STRING;
                        class = "";
                        cacheable = TRUE;
                        justify = LEFT;
                        visibility = TRUE;
                        angle = 0;
                        length = 20;
                        decimals = 0;
                        sortdirection = NONE;
                        fontname = "romsim";
                        fontcolor = 161;
                        fonttype = 4;
                        fontsize = 3;
                        fontratio = 0.8;
                        fontstyle = 0;
                        fontslant = 0;
                        pen = -1;
                        oncombine = NONE;
                    };

                    valuefield _tmp_3365
                    {
                        name = "ValueField_5";
                        location = (63.99658203125, 0.79999999999999);
                        formula = "GetValue(\"MATERIAL\")";
                        datatype = STRING;
                        class = "";
                        cacheable = TRUE;
                        justify = CENTERED;
                        visibility = TRUE;
                        angle = 0;
                        length = 7;
                        decimals = 0;
                        sortdirection = ASCENDING;
                        fontname = "romsim";
                        fontcolor = 161;
                        fonttype = 4;
                        fontsize = 3;
                        fontratio = 0.8;
                        fontstyle = 0;
                        fontslant = 0;
                        pen = -1;
                        oncombine = NONE;
                    };

                    valuefield _tmp_7111
                    {
                        name = "type";
                        location = (4, 0.79999999999999);
                        formula = "GetValue(\"TYPE\")";
                        datatype = STRING;
                        class = "";
                        cacheable = TRUE;
                        justify = LEFT;
                        visibility = FALSE;
                        angle = 0;
                        length = 8;
                        sortdirection = NONE;
                        fontname = "Arial";
                        fontcolor = 161;
                        fonttype = 2;
                        fontsize = 3;
                        fontratio = 1;
                        fontstyle = 0;
                        fontslant = 0;
                        pen = -1;
                        oncombine = NONE;
                    };

                    valuefield _tmp_9791
                    {
                        name = "DIA";
                        location = (118.5, 0.79999999999999);
                        formula = "GetValue(\"DIAMETER\")";
                        maxnumoflines = 1;
                        datatype = DOUBLE;
                        class = "Length";
                        cacheable = TRUE;
                        formatzeroasempty = FALSE;
                        justify = RIGHT;
                        visibility = FALSE;
                        angle = 0;
                        length = 5;
                        decimals = 0;
                        sortdirection = ASCENDING;
                        fontname = "Arial";
                        fontcolor = 161;
                        fonttype = 2;
                        fontsize = 3;
                        fontratio = 1;
                        fontstyle = 0;
                        fontslant = 0;
                        pen = -1;
                        oncombine = NONE;
                        unit = "mm";
                    };

                    lineorarc _tmp_955
                    {
                        name = "LineOrArc_i15";
                        x1 = 118.994355598446;
                        y1 = 1.77635683940025e-15;
                        x2 = 118.994355598446;
                        y2 = 5;
                        pen = -1;
                        color = 153;
                        linetype = 1;
                        linewidth = 1;
                        bulge = 0;
                    };

                    lineorarc _tmp_956
                    {
                        name = "LineOrArc_18";
                        x1 = 0;
                        y1 = 1.77635683940025e-15;
                        x2 = 142;
                        y2 = 1.77635683940025e-15;
                        pen = -1;
                        color = 153;
                        linetype = 1;
                        linewidth = 1;
                        bulge = 0;
                    };

                    lineorarc _tmp_957
                    {
                        name = "LineOrArc_26";
                        x1 = 0;
                        y1 = 5;
                        x2 = 142;
                        y2 = 5;
                        pen = -1;
                        color = 153;
                        linetype = 1;
                        linewidth = 1;
                        bulge = 0;
                    };

                    lineorarc _tmp_958
                    {
                        name = "LineOrArc_i16";
                        x1 = 101.617070530028;
                        y1 = 1.77635683940025e-15;
                        x2 = 101.617070530028;
                        y2 = 5;
                        pen = -1;
                        color = 153;
                        linetype = 1;
                        linewidth = 1;
                        bulge = 0;
                    };

                    lineorarc _tmp_959
                    {
                        name = "LineOrArc_i17";
                        x1 = 79.4397964385726;
                        y1 = 1.77635683940025e-15;
                        x2 = 79.4397964385726;
                        y2 = 5;
                        pen = -1;
                        color = 153;
                        linetype = 1;
                        linewidth = 1;
                        bulge = 0;
                    };

                    lineorarc _tmp_960
                    {
                        name = "LineOrArc_i18";
                        x1 = 62.3885297500254;
                        y1 = 1.77635683940025e-15;
                        x2 = 62.3885297500254;
                        y2 = 5;
                        pen = -1;
                        color = 153;
                        linetype = 1;
                        linewidth = 1;
                        bulge = 0;
                    };

                    lineorarc _tmp_961
                    {
                        name = "LineOrArc_i6";
                        x1 = 22.1726125811006;
                        y1 = 1.77635683940025e-15;
                        x2 = 22.1726125811006;
                        y2 = 5;
                        pen = -1;
                        color = 153;
                        linetype = 1;
                        linewidth = 1;
                        bulge = 0;
                    };

                    lineorarc _tmp_962
                    {
                        name = "LineOrArc_i7";
                        x1 = 0;
                        y1 = 1.77635683940025e-15;
                        x2 = 0;
                        y2 = 5;
                        pen = -1;
                        color = 153;
                        linetype = 1;
                        linewidth = 1;
                        bulge = 0;
                    };
                };

                row _tmp_5690
                {
                    name = "STUD";
                    height = 5;
                    visibility = TRUE;
                    usecolumns = FALSE;
                    rule = "";
                    contenttype = "STUD";
                    sorttype = COMBINE;

                    text _tmp_5722
                    {
                        name = "Text";
                        x1 = 65.2969678916176;
                        y1 = 1.10391372470575;
                        x2 = 65.2969678916176;
                        y2 = 1.10391372470575;
                        string = "GRADE";
                        fontname = "romsim";
                        fontcolor = 153;
                        fonttype = 4;
                        fontsize = 3;
                        fontratio = 0.8;
                        fontslant = 0;
                        fontstyle = 0;
                        angle = 0;
                        justify = CENTERED;
                        pen = -1;
                    };

                    text _tmp_5723
                    {
                        name = "Text_1";
                        x1 = 31;
                        y1 = 0.999999999999988;
                        x2 = 31;
                        y2 = 0.999999999999988;
                        string = "STUD BOLTS";
                        fontname = "romsim";
                        fontcolor = 153;
                        fonttype = 4;
                        fontsize = 3;
                        fontratio = 0.8;
                        fontslant = 0;
                        fontstyle = 0;
                        angle = 0;
                        justify = LEFT;
                        pen = -1;
                    };

                    group _tmp_999
                    {
                        name = "Group_2";

                        lineorarc _tmp_1000
                        {
                            name = "LineOrArc";
                            x1 = 118.994355598446;
                            y1 = 0;
                            x2 = 118.994355598446;
                            y2 = 5;
                            pen = -1;
                            color = 153;
                            linetype = 1;
                            linewidth = 1;
                            bulge = 0;
                        };

                        lineorarc _tmp_1001
                        {
                            name = "LineOrArc";
                            x1 = 0;
                            y1 = 0;
                            x2 = 142;
                            y2 = 0;
                            pen = -1;
                            color = 153;
                            linetype = 1;
                            linewidth = 1;
                            bulge = 0;
                        };

                        lineorarc _tmp_1002
                        {
                            name = "LineOrArc";
                            x1 = 0;
                            y1 = 5;
                            x2 = 142;
                            y2 = 5;
                            pen = -1;
                            color = 153;
                            linetype = 1;
                            linewidth = 1;
                            bulge = 0;
                        };

                        lineorarc _tmp_1003
                        {
                            name = "LineOrArc";
                            x1 = 101.617070530028;
                            y1 = 0;
                            x2 = 101.617070530028;
                            y2 = 5;
                            pen = -1;
                            color = 153;
                            linetype = 1;
                            linewidth = 1;
                            bulge = 0;
                        };

                        lineorarc _tmp_1004
                        {
                            name = "LineOrArc";
                            x1 = 79.4397964385726;
                            y1 = 0;
                            x2 = 79.4397964385726;
                            y2 = 5;
                            pen = -1;
                            color = 153;
                            linetype = 1;
                            linewidth = 1;
                            bulge = 0;
                        };

                        lineorarc _tmp_1005
                        {
                            name = "LineOrArc";
                            x1 = 62.3885297500254;
                            y1 = 0;
                            x2 = 62.3885297500254;
                            y2 = 5;
                            pen = -1;
                            color = 153;
                            linetype = 1;
                            linewidth = 1;
                            bulge = 0;
                        };

                        lineorarc _tmp_1006
                        {
                            name = "LineOrArc";
                            x1 = 22.1726125811006;
                            y1 = 0;
                            x2 = 22.1726125811006;
                            y2 = 5;
                            pen = -1;
                            color = 153;
                            linetype = 1;
                            linewidth = 1;
                            bulge = 0;
                        };

                        lineorarc _tmp_1007
                        {
                            name = "LineOrArc";
                            x1 = 0;
                            y1 = 0;
                            x2 = 0;
                            y2 = 5;
                            pen = -1;
                            color = 153;
                            linetype = 1;
                            linewidth = 1;
                            bulge = 0;
                        };
                    };
                };

                row _tmp_5705
                {
                    name = "STUD1";
                    height = 5;
                    visibility = TRUE;
                    usecolumns = FALSE;
                    rule = "";
                    contenttype = "STUD";
                    sorttype = COMBINE;

                    valuefield _tmp_5714
                    {
                        name = "NUMBER1";
                        location = (128.49853515625, 0.999999999999988);
                        formula = "GetValue(\"NUMBER\")";
                        maxnumoflines = 1;
                        datatype = INTEGER;
                        class = "";
                        cacheable = TRUE;
                        formatzeroasempty = FALSE;
                        justify = RIGHT;
                        visibility = FALSE;
                        angle = 0;
                        length = 5;
                        sortdirection = NONE;
                        fontname = "Arial";
                        fontcolor = 161;
                        fonttype = 2;
                        fontsize = 3;
                        fontratio = 1;
                        fontstyle = 0;
                        fontslant = 0;
                        pen = -1;
                        oncombine = SUM;
                    };

                    valuefield _tmp_5715
                    {
                        name = "LENGTH1";
                        location = (104.49755859375, 0.999999999999988);
                        formula = "GetValue(\"LENGTH\")";
                        datatype = DOUBLE;
                        class = "Length";
                        cacheable = TRUE;
                        justify = RIGHT;
                        visibility = TRUE;
                        angle = 0;
                        length = 5;
                        decimals = 0;
                        sortdirection = ASCENDING;
                        fontname = "Arial";
                        fontcolor = 161;
                        fonttype = 2;
                        fontsize = 3;
                        fontratio = 1;
                        fontstyle = 0;
                        fontslant = 0;
                        pen = -1;
                        oncombine = NONE;
                        unit = "mm";
                    };

                    valuefield _tmp_5716
                    {
                        name = "STUD_BOLT";
                        location = (22.5, 0.999999999999988);
                        formula = "Sum(\"NUMBER1\")+\" - M\"+int(GetFieldFormula(\"DIA1\"))+\" \"+GetFieldFormula(\"NAME1\")+\" \"+\"X\"+\" \"+int(GetFieldFormula(\"LENGTH1\"))";
                        datatype = STRING;
                        class = "";
                        cacheable = TRUE;
                        justify = LEFT;
                        visibility = TRUE;
                        angle = 0;
                        length = 20;
                        decimals = 0;
                        sortdirection = NONE;
                        fontname = "romsim";
                        fontcolor = 161;
                        fonttype = 4;
                        fontsize = 3;
                        fontratio = 0.8;
                        fontstyle = 0;
                        fontslant = 0;
                        pen = -1;
                        oncombine = NONE;
                    };

                    valuefield _tmp_5717
                    {
                        name = "ValueField_12";
                        location = (63.99658203125, 0.999999999999988);
                        formula = "GetValue(\"MATERIAL\")";
                        datatype = STRING;
                        class = "";
                        cacheable = TRUE;
                        justify = CENTERED;
                        visibility = TRUE;
                        angle = 0;
                        length = 7;
                        decimals = 0;
                        sortdirection = ASCENDING;
                        fontname = "romsim";
                        fontcolor = 161;
                        fonttype = 4;
                        fontsize = 3;
                        fontratio = 0.8;
                        fontstyle = 0;
                        fontslant = 0;
                        pen = -1;
                        oncombine = NONE;
                    };

                    valuefield _tmp_5718
                    {
                        name = "NAME1";
                        location = (4, 0.999999999999988);
                        formula = "GetValue(\"NAME_SHORT\")";
                        datatype = STRING;
                        class = "";
                        cacheable = TRUE;
                        justify = LEFT;
                        visibility = FALSE;
                        angle = 0;
                        length = 8;
                        sortdirection = NONE;
                        fontname = "Arial";
                        fontcolor = 161;
                        fonttype = 2;
                        fontsize = 3;
                        fontratio = 1;
                        fontstyle = 0;
                        fontslant = 0;
                        pen = -1;
                        oncombine = NONE;
                    };

                    valuefield _tmp_5719
                    {
                        name = "DIA1";
                        location = (118.5, 0.999999999999988);
                        formula = "GetValue(\"DIAMETER\")";
                        maxnumoflines = 1;
                        datatype = DOUBLE;
                        class = "Length";
                        cacheable = TRUE;
                        formatzeroasempty = FALSE;
                        justify = RIGHT;
                        visibility = FALSE;
                        angle = 0;
                        length = 5;
                        decimals = 0;
                        sortdirection = ASCENDING;
                        fontname = "Arial";
                        fontcolor = 161;
                        fonttype = 2;
                        fontsize = 3;
                        fontratio = 1;
                        fontstyle = 0;
                        fontslant = 0;
                        pen = -1;
                        oncombine = NONE;
                        unit = "mm";
                    };

                    group _tmp_1017
                    {
                        name = "Group_3";

                        lineorarc _tmp_1018
                        {
                            name = "LineOrArc";
                            x1 = 118.994355598446;
                            y1 = 0;
                            x2 = 118.994355598446;
                            y2 = 5;
                            pen = -1;
                            color = 153;
                            linetype = 1;
                            linewidth = 1;
                            bulge = 0;
                        };

                        lineorarc _tmp_1019
                        {
                            name = "LineOrArc";
                            x1 = 0;
                            y1 = 0;
                            x2 = 142;
                            y2 = 0;
                            pen = -1;
                            color = 153;
                            linetype = 1;
                            linewidth = 1;
                            bulge = 0;
                        };

                        lineorarc _tmp_1020
                        {
                            name = "LineOrArc";
                            x1 = 0;
                            y1 = 5;
                            x2 = 142;
                            y2 = 5;
                            pen = -1;
                            color = 153;
                            linetype = 1;
                            linewidth = 1;
                            bulge = 0;
                        };

                        lineorarc _tmp_1021
                        {
                            name = "LineOrArc";
                            x1 = 101.617070530028;
                            y1 = 0;
                            x2 = 101.617070530028;
                            y2 = 5;
                            pen = -1;
                            color = 153;
                            linetype = 1;
                            linewidth = 1;
                            bulge = 0;
                        };

                        lineorarc _tmp_1022
                        {
                            name = "LineOrArc";
                            x1 = 79.4397964385726;
                            y1 = 0;
                            x2 = 79.4397964385726;
                            y2 = 5;
                            pen = -1;
                            color = 153;
                            linetype = 1;
                            linewidth = 1;
                            bulge = 0;
                        };

                        lineorarc _tmp_1023
                        {
                            name = "LineOrArc";
                            x1 = 62.3885297500254;
                            y1 = 0;
                            x2 = 62.3885297500254;
                            y2 = 5;
                            pen = -1;
                            color = 153;
                            linetype = 1;
                            linewidth = 1;
                            bulge = 0;
                        };

                        lineorarc _tmp_1024
                        {
                            name = "LineOrArc";
                            x1 = 22.1726125811006;
                            y1 = 0;
                            x2 = 22.1726125811006;
                            y2 = 5;
                            pen = -1;
                            color = 153;
                            linetype = 1;
                            linewidth = 1;
                            bulge = 0;
                        };

                        lineorarc _tmp_1025
                        {
                            name = "LineOrArc";
                            x1 = 0;
                            y1 = 0;
                            x2 = 0;
                            y2 = 5;
                            pen = -1;
                            color = 153;
                            linetype = 1;
                            linewidth = 1;
                            bulge = 0;
                        };
                    };
                };
            };
        };
    };
};
