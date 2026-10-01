template _tmp_883
{
    name = "tpled_template1";
    type = GRAPHICAL;
    width = 65.0184950513656;
    maxheight = 120;
    columns = (1, 1);
    gap = 5;
    fillpolicy = EVEN;
    filldirection = HORIZONTAL;
    fillstartfrom = TOPLEFT;
    margins = (0, 0, 0, 0);
    gridxspacing = 1;
    gridyspacing = 1;
    version = 4;
    created = "25.11.2009 20:13";
    modified = "17.07.2020 14:18";
    notes = "";
    colors = "153;152;160;161;162;163;164;165;154;155;156;157;158;159;130;131;132;133;";

    pageheader _tmp_0
    {
        name = "PageHeader";
        height = 5;
        outputpolicy = NONE;

        rectangle _tmp_1
        {
            name = "Rectangle";
            x1 = -0;
            y1 = 0;
            x2 = 65;
            y2 = 5;
            filled = FALSE;
            filltype = -1;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
        };

        text _tmp_3
        {
            name = "Text_1";
            x1 = 15;
            y1 = 1;
            x2 = 15;
            y2 = 1;
            string = "CONCRETE QUANTITY";
            fontname = "romsim";
            fontcolor = 153;
            fonttype = 4;
            fontsize = 2.5;
            fontratio = 0.85;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };
    };

    row _tmp_913
    {
        name = "CAST_UNIT";
        height = 5;
        visibility = TRUE;
        usecolumns = FALSE;
        rule = "";
        contenttype = "CAST_UNIT";
        sorttype = COMBINE;

        text _tmp_922
        {
            name = "3";
            x1 = 53;
            y1 = 3;
            x2 = 53;
            y2 = 3;
            string = "3";
            fontname = "romsim";
            fontcolor = 161;
            fonttype = 4;
            fontsize = 1.5;
            fontratio = 1;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        text _tmp_923
        {
            name = "M";
            x1 = 51.1593329456832;
            y1 = 1.12239728996585;
            x2 = 51.1593329456832;
            y2 = 1.12239728996585;
            string = "M";
            fontname = "romsim";
            fontcolor = 161;
            fonttype = 4;
            fontsize = 2;
            fontratio = 0.85;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        text _tmp_895
        {
            name = "Text";
            x1 = 5.29011645825994;
            y1 = 1.41360294117647;
            x2 = 5.29011645825994;
            y2 = 1.41360294117647;
            string = "fck=      Mpa";
            fontname = "romsim";
            fontcolor = 161;
            fonttype = 4;
            fontsize = 2;
            fontratio = 0.85;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        valuefield _tmp_896
        {
            name = "MATERIAL_field";
            location = (10.8244980626981, 1.27757352941176);
            formula = "mid(GetValue(\"MATERIAL\"), 1 , 2)";
            maxnumoflines = 1;
            datatype = INTEGER;
            class = "";
            cacheable = TRUE;
            formatzeroasempty = FALSE;
            justify = CENTERED;
            visibility = TRUE;
            angle = 0;
            length = 5;
            decimals = 2;
            sortdirection = ASCENDING;
            fontname = "romsim";
            fontcolor = 161;
            fonttype = 4;
            fontsize = 2;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
            aligncontenttotop = FALSE;
        };

        valuefield _tmp_893
        {
            name = "ValueField";
            location = (36.4607989902547, 1.18235294117647);
            formula = "GetValue(\"VOLUME\")";
            maxnumoflines = 1;
            datatype = DOUBLE;
            class = "Volume";
            cacheable = TRUE;
            formatzeroasempty = FALSE;
            justify = CENTERED;
            visibility = TRUE;
            angle = 0;
            length = 8;
            decimals = 2;
            sortdirection = NONE;
            fontname = "romsim";
            fontcolor = 161;
            fonttype = 4;
            fontsize = 2;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
            unit = "m3";
        };

        rectangle _tmp_5
        {
            name = "Rectangle_1";
            x1 = 0;
            y1 = 0;
            x2 = 65;
            y2 = 5;
            filled = FALSE;
            filltype = -1;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
        };

        lineorarc _tmp_12
        {
            name = "LineOrArc";
            x1 = 31;
            y1 = 5;
            x2 = 31;
            y2 = 0;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };
    };
};
