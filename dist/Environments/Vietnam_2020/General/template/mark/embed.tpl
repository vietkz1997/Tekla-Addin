template _tmp_843
{
    name = "template1";
    type = GRAPHICAL;
    width = 52;
    maxheight = 120;
    columns = (1, 1);
    gap = 5;
    fillpolicy = EVEN;
    filldirection = HORIZONTAL;
    margins = (0, 0, 0, 0);
    gridxspacing = 1;
    gridyspacing = 1;
    version = 3.21;
    created = "25.08.2008 11:51";
    modified = "27.04.2011 16:16";
    notes = "";

    row _tmp_869
    {
        name = "ASSEMBLY";
        height = 6;
        visibility = TRUE;
        usecolumns = FALSE;
        rule = "";
        contenttype = "ASSEMBLY";
        sorttype = COMBINE;

        valuefield _tmp_870
        {
            name = "MAINPART.NAME_field";
            location = (2, 1);
            formula = "GetValue(\"MAINPART.NAME\")";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 14;
            decimals = 0;
            sortdirection = NONE;
            fontname = "Arial";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1.5;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        lineorarc _tmp_871
        {
            name = "LineOrArc";
            x1 = 0;
            y1 = 6;
            x2 = 52;
            y2 = 6;
            pen = -1;
            color = 164;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_873
        {
            name = "LineOrArc_1";
            x1 = 0;
            y1 = 0;
            x2 = 52;
            y2 = -0;
            pen = -1;
            color = 164;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_874
        {
            name = "LineOrArc_2";
            x1 = 0;
            y1 = 6;
            x2 = 0;
            y2 = 0;
            pen = -1;
            color = 164;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_875
        {
            name = "LineOrArc_3";
            x1 = 52;
            y1 = 6;
            x2 = 52;
            y2 = 0;
            pen = -1;
            color = 164;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        row _tmp_1196
        {
            name = "PART";
            height = 4;
            visibility = TRUE;
            usecolumns = FALSE;
            rule = "";
            contenttype = "PART";
            sorttype = COMBINE;

            lineorarc _tmp_880
            {
                name = "LineOrArc_4";
                x1 = 0;
                y1 = 4;
                x2 = 0;
                y2 = 0;
                pen = -1;
                color = 164;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_881
            {
                name = "LineOrArc_5";
                x1 = 52;
                y1 = 4;
                x2 = 52;
                y2 = 0;
                pen = -1;
                color = 164;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_882
            {
                name = "LineOrArc_6";
                x1 = 0;
                y1 = 4;
                x2 = 52;
                y2 = 4;
                pen = -1;
                color = 164;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_883
            {
                name = "LineOrArc_7";
                x1 = 0;
                y1 = 0;
                x2 = 52;
                y2 = -0;
                pen = -1;
                color = 164;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            valuefield _tmp_884
            {
                name = "PROFILE_field";
                location = (2, 1);
                formula = "GetValue(\"PROFILE\")";
                datatype = STRING;
                class = "";
                cacheable = TRUE;
                justify = LEFT;
                visibility = TRUE;
                angle = 0;
                length = 14;
                decimals = 0;
                sortdirection = NONE;
                fontname = "Arial";
                fontcolor = 153;
                fonttype = 2;
                fontsize = 2;
                fontratio = 1.5;
                fontstyle = 0;
                fontslant = 0;
                pen = -1;
                oncombine = NONE;
            };

            valuefield _tmp_885
            {
                name = "LENGTH_field_1";
                location = (34, 1);
                formula = "GetValue(\"LENGTH\")";
                datatype = DOUBLE;
                class = "Length";
                cacheable = TRUE;
                justify = LEFT;
                visibility = TRUE;
                angle = 0;
                length = 8;
                decimals = 1;
                sortdirection = NONE;
                fontname = "Arial";
                fontcolor = 153;
                fonttype = 2;
                fontsize = 2;
                fontratio = 1.5;
                fontstyle = 0;
                fontslant = 0;
                pen = -1;
                oncombine = NONE;
                unit = "mm";
            };

            text _tmp_886
            {
                name = "L=";
                x1 = 30;
                y1 = 1;
                x2 = 30;
                y2 = 1;
                string = "L=";
                fontname = "Arial";
                fontcolor = 153;
                fonttype = 2;
                fontsize = 2;
                fontratio = 1;
                fontslant = 0;
                fontstyle = 0;
                angle = 0;
                justify = LEFT;
                pen = -1;
            };

            row _tmp_984
            {
                name = "Row";
                height = 4;
                visibility = TRUE;
                usecolumns = FALSE;
                rule = "";
                contenttype = "REBAR";
                sorttype = COMBINE;

                lineorarc _tmp_988
                {
                    name = "LineOrArc_8";
                    x1 = 0;
                    y1 = 4;
                    x2 = 0;
                    y2 = 0;
                    pen = -1;
                    color = 164;
                    linetype = 1;
                    linewidth = 1;
                    bulge = 0;
                };

                lineorarc _tmp_989
                {
                    name = "LineOrArc_9";
                    x1 = 52;
                    y1 = 4;
                    x2 = 52;
                    y2 = 0;
                    pen = -1;
                    color = 164;
                    linetype = 1;
                    linewidth = 1;
                    bulge = 0;
                };

                lineorarc _tmp_990
                {
                    name = "LineOrArc_10";
                    x1 = 0;
                    y1 = 4;
                    x2 = 52;
                    y2 = 4;
                    pen = -1;
                    color = 164;
                    linetype = 1;
                    linewidth = 1;
                    bulge = 0;
                };

                lineorarc _tmp_991
                {
                    name = "LineOrArc_11";
                    x1 = 0;
                    y1 = 0;
                    x2 = 52;
                    y2 = -0;
                    pen = -1;
                    color = 164;
                    linetype = 1;
                    linewidth = 1;
                    bulge = 0;
                };

                valuefield _tmp_992
                {
                    name = "ValueField_2";
                    location = (11, 1);
                    formula = "GetValue(\"SIZE\")";
                    datatype = STRING;
                    class = "";
                    cacheable = TRUE;
                    justify = LEFT;
                    visibility = TRUE;
                    angle = 0;
                    length = 2;
                    decimals = 0;
                    sortdirection = NONE;
                    fontname = "Arial";
                    fontcolor = 153;
                    fonttype = 2;
                    fontsize = 2;
                    fontratio = 1.5;
                    fontstyle = 0;
                    fontslant = 0;
                    pen = -1;
                    oncombine = NONE;
                };

                valuefield _tmp_993
                {
                    name = "ValueField_3";
                    location = (21, 1);
                    formula = "GetValue(\"LENGTH\")";
                    datatype = DOUBLE;
                    class = "Length";
                    cacheable = TRUE;
                    justify = LEFT;
                    visibility = TRUE;
                    angle = 0;
                    length = 4;
                    decimals = 0;
                    sortdirection = NONE;
                    fontname = "Arial";
                    fontcolor = 153;
                    fonttype = 2;
                    fontsize = 2;
                    fontratio = 1.5;
                    fontstyle = 0;
                    fontslant = 0;
                    pen = -1;
                    oncombine = NONE;
                    unit = "mm";
                };

                text _tmp_994
                {
                    name = "Text";
                    x1 = 18;
                    y1 = 1;
                    x2 = 18;
                    y2 = 1;
                    string = "L=";
                    fontname = "Arial";
                    fontcolor = 153;
                    fonttype = 2;
                    fontsize = 2;
                    fontratio = 1;
                    fontslant = 0;
                    fontstyle = 0;
                    angle = 0;
                    justify = LEFT;
                    pen = -1;
                };

                text _tmp_1186
                {
                    name = "Rebar";
                    x1 = 2;
                    y1 = 1;
                    x2 = 2;
                    y2 = 1;
                    string = "Rebar";
                    fontname = "Arial";
                    fontcolor = 153;
                    fonttype = 2;
                    fontsize = 2;
                    fontratio = 1;
                    fontslant = 0;
                    fontstyle = 0;
                    angle = 0;
                    justify = LEFT;
                    pen = -1;
                };

                valuefield _tmp_1492
                {
                    name = "GRADE_field";
                    location = (30, 1);
                    formula = "GetValue(\"GRADE\")";
                    datatype = STRING;
                    class = "";
                    cacheable = TRUE;
                    justify = LEFT;
                    visibility = TRUE;
                    angle = 0;
                    length = 8;
                    decimals = 0;
                    sortdirection = NONE;
                    fontname = "Arial";
                    fontcolor = 153;
                    fonttype = 2;
                    fontsize = 2;
                    fontratio = 1.5;
                    fontstyle = 0;
                    fontslant = 0;
                    pen = -1;
                    oncombine = NONE;
                };
            };

            row _tmp_1271
            {
                name = "Row_1";
                height = 4;
                visibility = TRUE;
                usecolumns = FALSE;
                rule = "";
                contenttype = "STUD";
                sorttype = COMBINE;

                lineorarc _tmp_1272
                {
                    name = "LineOrArc_12";
                    x1 = 0;
                    y1 = 4;
                    x2 = 0;
                    y2 = 0;
                    pen = -1;
                    color = 164;
                    linetype = 1;
                    linewidth = 1;
                    bulge = 0;
                };

                lineorarc _tmp_1273
                {
                    name = "LineOrArc_13";
                    x1 = 52;
                    y1 = 4;
                    x2 = 52;
                    y2 = -0;
                    pen = -1;
                    color = 164;
                    linetype = 1;
                    linewidth = 1;
                    bulge = 0;
                };

                lineorarc _tmp_1274
                {
                    name = "LineOrArc_14";
                    x1 = 0;
                    y1 = 4;
                    x2 = 52;
                    y2 = 4;
                    pen = -1;
                    color = 164;
                    linetype = 1;
                    linewidth = 1;
                    bulge = 0;
                };

                lineorarc _tmp_1275
                {
                    name = "LineOrArc_15";
                    x1 = 0;
                    y1 = 0;
                    x2 = 52;
                    y2 = -0;
                    pen = -1;
                    color = 164;
                    linetype = 1;
                    linewidth = 1;
                    bulge = 0;
                };

                valuefield _tmp_1276
                {
                    name = "ValueField";
                    location = (31, 1);
                    formula = "GetValue(\"GRADE\")";
                    datatype = STRING;
                    class = "";
                    cacheable = TRUE;
                    justify = LEFT;
                    visibility = TRUE;
                    angle = 0;
                    length = 10;
                    decimals = 0;
                    sortdirection = NONE;
                    fontname = "Arial";
                    fontcolor = 153;
                    fonttype = 2;
                    fontsize = 2;
                    fontratio = 1.5;
                    fontstyle = 0;
                    fontslant = 0;
                    pen = -1;
                    oncombine = NONE;
                };

                valuefield _tmp_1277
                {
                    name = "ValueField_1";
                    location = (25, 1);
                    formula = "GetValue(\"LENGTH\")";
                    datatype = DOUBLE;
                    class = "Length";
                    cacheable = TRUE;
                    justify = RIGHT;
                    visibility = TRUE;
                    angle = 0;
                    length = 3;
                    decimals = 1;
                    sortdirection = NONE;
                    fontname = "Arial";
                    fontcolor = 153;
                    fonttype = 2;
                    fontsize = 2;
                    fontratio = 1.5;
                    fontstyle = 0;
                    fontslant = 0;
                    pen = -1;
                    oncombine = NONE;
                    unit = "mm";
                };

                text _tmp_1279
                {
                    name = "Text_2";
                    x1 = 2;
                    y1 = 1;
                    x2 = 2;
                    y2 = 1;
                    string = "Studs";
                    fontname = "Arial";
                    fontcolor = 153;
                    fonttype = 2;
                    fontsize = 2;
                    fontratio = 1;
                    fontslant = 0;
                    fontstyle = 0;
                    angle = 0;
                    justify = LEFT;
                    pen = -1;
                };

                valuefield _tmp_1491
                {
                    name = "DIAMETER_field";
                    location = (19, 1);
                    formula = "GetValue(\"DIAMETER\")";
                    datatype = DOUBLE;
                    class = "Length";
                    cacheable = TRUE;
                    justify = RIGHT;
                    visibility = TRUE;
                    angle = 0;
                    length = 2;
                    decimals = 0;
                    sortdirection = NONE;
                    fontname = "Arial";
                    fontcolor = 153;
                    fonttype = 2;
                    fontsize = 2;
                    fontratio = 1.5;
                    fontstyle = 0;
                    fontslant = 0;
                    pen = -1;
                    oncombine = NONE;
                    unit = "mm";
                };

                valuefield _tmp_1493
                {
                    name = "NUMBER_field";
                    location = (10, 1);
                    formula = "GetValue(\"NUMBER\")";
                    datatype = INTEGER;
                    class = "";
                    cacheable = TRUE;
                    justify = LEFT;
                    visibility = TRUE;
                    angle = 0;
                    length = 1;
                    decimals = 0;
                    sortdirection = NONE;
                    fontname = "Arial";
                    fontcolor = 153;
                    fonttype = 2;
                    fontsize = 2;
                    fontratio = 1.5;
                    fontstyle = 0;
                    fontslant = 0;
                    pen = -1;
                    oncombine = NONE;
                };

                text _tmp_1549
                {
                    name = "x";
                    x1 = 13;
                    y1 = 1;
                    x2 = 13;
                    y2 = 1;
                    string = "pcs";
                    fontname = "Arial";
                    fontcolor = 153;
                    fonttype = 2;
                    fontsize = 2;
                    fontratio = 1;
                    fontslant = 0;
                    fontstyle = 0;
                    angle = 0;
                    justify = LEFT;
                    pen = -1;
                };

                text _tmp_1625
                {
                    name = "-";
                    x1 = 23;
                    y1 = 1;
                    x2 = 23;
                    y2 = 1;
                    string = "x";
                    fontname = "Arial";
                    fontcolor = 153;
                    fonttype = 2;
                    fontsize = 2;
                    fontratio = 1;
                    fontslant = 0;
                    fontstyle = 0;
                    angle = 0;
                    justify = LEFT;
                    pen = -1;
                };
            };
        };
    };
};
