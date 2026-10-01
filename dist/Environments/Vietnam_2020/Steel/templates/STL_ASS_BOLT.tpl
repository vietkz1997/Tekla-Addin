
template _tmp_843
{
    name = "template1";
    type = GRAPHICAL;
    width = 142.064896713962;
    maxheight = 120;
    columns = (1, 1);
    gap = 5;
    fillpolicy = EVEN;
    filldirection = HORIZONTAL;
    margins = (0, 0, 0, 0);
    gridxspacing = 1;
    gridyspacing = 1;
    version = 3.21;
    created = "26.12.2008 14:33";
    modified = "26.05.2011 10:38";
    notes = "";

    header _tmp_1135
    {
        name = "Header";
        height = 5;

        text _tmp_1177
        {
            name = "H594X302X14X23_2";
            x1 = 30.7030321083824;
            y1 = 0.89608627529424;
            x2 = 30.7030321083824;
            y2 = 0.89608627529424;
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

        text _tmp_1196
        {
            name = "Text";
            x1 = 67;
            y1 = 1;
            x2 = 67;
            y2 = 1;
            string = "Q'TY";
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

        text _tmp_1198
        {
            name = "Text_1";
            x1 = 83.886962890625;
            y1 = 1;
            x2 = 83.886962890625;
            y2 = 1;
            string = "LENGTH";
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

        text _tmp_1200
        {
            name = "Text_2";
            x1 = 104;
            y1 = 1;
            x2 = 104;
            y2 = 1;
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

        group _tmp_1176
        {
            name = "Group_2";

            lineorarc _tmp_1177
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

            lineorarc _tmp_1178
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

            lineorarc _tmp_1179
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

            lineorarc _tmp_1180
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

            lineorarc _tmp_1181
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

            lineorarc _tmp_1182
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

            lineorarc _tmp_1183
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

            lineorarc _tmp_1184
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

    row _tmp_870
    {
        name = "ASSEMBLY";
        height = 1;
        visibility = FALSE;
        usecolumns = FALSE;
        rule = "";
        contenttype = "ASSEMBLY";
        sorttype = COMBINE;

        row _tmp_3819
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

                    valuefield _tmp_1755
                    {
                        name = "ValueField_2";
                        location = (31, 1);
                        formula = "\"M \"+int(GetValue(\"DIAMETER\"))+\" \"+GetValue(\"TYPE\")";
                        datatype = STRING;
                        class = "";
                        cacheable = TRUE;
                        justify = LEFT;
                        visibility = TRUE;
                        angle = 0;
                        length = 12;
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

                    valuefield _tmp_2019
                    {
                        name = "ValueField_3";
                        location = (65.99853515625, 1);
                        formula = "GetValue(\"NUMBER\")";
                        datatype = INTEGER;
                        class = "";
                        cacheable = TRUE;
                        justify = RIGHT;
                        visibility = TRUE;
                        angle = 0;
                        length = 5;
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
                        oncombine = SUM;
                    };

                    valuefield _tmp_2313
                    {
                        name = "ValueField_4";
                        location = (85, 1);
                        formula = "GetValue(\"LENGTH\")";
                        datatype = STRING;
                        class = "Length";
                        cacheable = TRUE;
                        justify = RIGHT;
                        visibility = TRUE;
                        angle = 0;
                        length = 5;
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

                    valuefield _tmp_3365
                    {
                        name = "ValueField_5";
                        location = (103.3, 1);
                        formula = "GetValue(\"MATERIAL\")";
                        datatype = STRING;
                        class = "";
                        cacheable = TRUE;
                        justify = CENTERED;
                        visibility = TRUE;
                        angle = 0;
                        length = 7;
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

                    group _tmp_1185
                    {
                        name = "Group";

                        lineorarc _tmp_1186
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

                        lineorarc _tmp_1187
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

                        lineorarc _tmp_1188
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

                        lineorarc _tmp_1189
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

                        lineorarc _tmp_1190
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

                        lineorarc _tmp_1191
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

                        lineorarc _tmp_1192
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

                        lineorarc _tmp_1193
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

                row _tmp_4086
                {
                    name = "STUD1";
                    height = 5;
                    visibility = TRUE;
                    usecolumns = FALSE;
                    rule = "";
                    contenttype = "STUD";
                    sorttype = COMBINE;

                    text _tmp_4379
                    {
                        name = "Text_3";
                        x1 = 30;
                        y1 = 1;
                        x2 = 30;
                        y2 = 1;
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

                    text _tmp_4678
                    {
                        name = "Text_4";
                        x1 = 67;
                        y1 = 1;
                        x2 = 67;
                        y2 = 1;
                        string = "Q'TY";
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

                    text _tmp_4679
                    {
                        name = "Text_5";
                        x1 = 83.886962890625;
                        y1 = 1;
                        x2 = 83.886962890625;
                        y2 = 1;
                        string = "LENGTH";
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

                    text _tmp_4680
                    {
                        name = "Text_6";
                        x1 = 104;
                        y1 = 1;
                        x2 = 104;
                        y2 = 1;
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

                    group _tmp_1194
                    {
                        name = "Group_1";

                        lineorarc _tmp_1195
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

                        lineorarc _tmp_1196
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

                        lineorarc _tmp_1197
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

                        lineorarc _tmp_1198
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

                        lineorarc _tmp_1199
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

                        lineorarc _tmp_1200
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

                        lineorarc _tmp_1201
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

                        lineorarc _tmp_1202
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

                row _tmp_4365
                {
                    name = "STUD2";
                    height = 5;
                    visibility = TRUE;
                    usecolumns = FALSE;
                    rule = "";
                    contenttype = "STUD";
                    sorttype = COMBINE;

                    valuefield _tmp_4374
                    {
                        name = "ValueField_8";
                        location = (30.9921875, 1);
                        formula = "\"M \"+int(GetValue(\"DIAMETER\"))+\" \"+GetValue(\"TYPE\")";
                        datatype = STRING;
                        class = "";
                        cacheable = TRUE;
                        justify = LEFT;
                        visibility = TRUE;
                        angle = 0;
                        length = 12;
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

                    valuefield _tmp_4375
                    {
                        name = "ValueField_9";
                        location = (65.99853515625, 1);
                        formula = "GetValue(\"NUMBER\")";
                        datatype = INTEGER;
                        class = "";
                        cacheable = TRUE;
                        justify = RIGHT;
                        visibility = TRUE;
                        angle = 0;
                        length = 5;
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
                        oncombine = SUM;
                    };

                    valuefield _tmp_4376
                    {
                        name = "ValueField_10";
                        location = (85, 1);
                        formula = "GetValue(\"LENGTH\")";
                        datatype = STRING;
                        class = "Length";
                        cacheable = TRUE;
                        justify = RIGHT;
                        visibility = TRUE;
                        angle = 0;
                        length = 5;
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

                    valuefield _tmp_4377
                    {
                        name = "ValueField_11";
                        location = (103.3, 1);
                        formula = "GetValue(\"MATERIAL\")";
                        datatype = STRING;
                        class = "";
                        cacheable = TRUE;
                        justify = CENTERED;
                        visibility = TRUE;
                        angle = 0;
                        length = 7;
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

                    group _tmp_1203
                    {
                        name = "Group_3";

                        lineorarc _tmp_1204
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

                        lineorarc _tmp_1205
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

                        lineorarc _tmp_1206
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

                        lineorarc _tmp_1207
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

                        lineorarc _tmp_1208
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

                        lineorarc _tmp_1209
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

                        lineorarc _tmp_1210
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

                        lineorarc _tmp_1211
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
