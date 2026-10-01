template _tmp_0
{
    name = "tpled_template1";
    type = GRAPHICAL;
    width = 85;
    maxheight = 160;
    columns = (1, 1);
    gap = 5;
    fillpolicy = EVEN;
    filldirection = HORIZONTAL;
    margins = (10, 10, 10, 10);
    gridxspacing = 5;
    gridyspacing = 5;
    version = 3.21;
    created = "02.06.2015 12:51";
    modified = "06.01.2016 11:29";
    notes = "";

    row _tmp_8
    {
        name = "Pour_number";
        height = 5;
        visibility = TRUE;
        usecolumns = TRUE;
        rule = "";
        contenttype = "POUR_OBJECT";
        sorttype = COMBINE;

        valuefield _tmp_12
        {
            name = "USERDEFINED.POUR_NUMBER_field";
            location = (1.328125, 0);
            formula = "GetValue(\"POUR_TYPE\") + \" #\" + GetValue(\"POUR_NUMBER\")";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 34;
            decimals = 0;
            sortdirection = ASCENDING;
            fontname = "Arial Narrow";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 3;
            fontratio = 1;
            fontstyle = 1;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };
    };

    row _tmp_19
    {
        name = "Pour_date";
        height = 4;
        visibility = TRUE;
        usecolumns = FALSE;
        rule = "if (IsSet(\"USERDEFINED.PLANNED_START_POUR\")) then\n  Output()\nelse\n  StepOver()\nendif";
        contenttype = "POUR_OBJECT";
        sorttype = COMBINE;

        valuefield _tmp_28
        {
            name = "albl_Pour_date";
            location = (1.328125, 0);
            formula = "GetValue(\"TranslatedText(\"albl_Pour_date\")\") + \": \" + format(GetValue(\"USERDEFINED.PLANNED_START_POUR\"),\"Date\",\"dd.mm.yyyy\",10)";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 42;
            decimals = 0;
            sortdirection = NONE;
            fontname = "Arial Narrow";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };
    };

    row _tmp_24
    {
        name = "Pour_volume";
        height = 4;
        visibility = TRUE;
        usecolumns = FALSE;
        rule = "";
        contenttype = "POUR_OBJECT";
        sorttype = COMBINE;

        valuefield _tmp_44
        {
            name = "albl_Volume";
            location = (1.328125, 0);
            formula = "GetValue(\"TranslatedText(\"albl_Volume\")\") + \": \" + format(GetValue(\"VOLUME\"),\"Volume\",\"m3\",2)";
            datatype = STRING;
            class = "Volume";
            cacheable = TRUE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 42;
            decimals = 0;
            sortdirection = NONE;
            fontname = "Arial Narrow";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };
    };

    row _tmp_34
    {
        name = "Pour_comment";
        height = 4;
        visibility = TRUE;
        usecolumns = FALSE;
        rule = "if (IsSet(\"USERDEFINED.COMMENT\")) then\n  Output()\nelse\n  StepOver()\nendif";
        contenttype = "POUR_OBJECT";
        sorttype = COMBINE;

        valuefield _tmp_40
        {
            name = "USERDEFINED.COMMENT";
            location = (1.2890625, 0);
            formula = "\"(\"+ GetValue(\"USERDEFINED.COMMENT\") +\")\"";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 42;
            decimals = 0;
            sortdirection = ASCENDING;
            fontname = "Arial Narrow";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };
    };
};
