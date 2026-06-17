
--add values in cargonameforurl column of cargotype table
update CargoType set CargoNameForUrl='gf' where CargoName='General Freight'
update CargoType set CargoNameForUrl='hg' where CargoName='Household Goods'
update CargoType set CargoNameForUrl='scr' where CargoName='Metal: Sheets,Coils, Rolls'
update CargoType set CargoNameForUrl='mv' where CargoName='Motor Vehicles'

update CargoType set CargoNameForUrl='lpbl' where CargoName='Logs, Poles, Beams, Lumber'
update CargoType set CargoNameForUrl='bm' where CargoName='Building, Materials'
update CargoType set CargoNameForUrl='mh' where CargoName='Mobile Homes'
update CargoType set CargoNameForUrl='mlo' where CargoName='Machinery, Large Objects'

update CargoType set CargoNameForUrl='fp' where CargoName='Fresh Produce'
update CargoType set CargoNameForUrl='lg' where CargoName='Liquids/Gases'
update CargoType set CargoNameForUrl='ic' where CargoName='Intermodal Containers'
update CargoType set CargoNameForUrl='o' where CargoName='OilfieldEquipment'

update CargoType set CargoNameForUrl='livestock' where CargoName='Livestock'
update CargoType set CargoNameForUrl='gfh' where CargoName='Grain, Feed, Hay'
update CargoType set CargoNameForUrl='c' where CargoName='Coal/Coke'
update CargoType set CargoNameForUrl='meat' where CargoName='Meat'

update CargoType set CargoNameForUrl='usm' where CargoName='U.S. Mail'
update CargoType set CargoNameForUrl='chemicals' where CargoName='Chemicals'
update CargoType set CargoNameForUrl='cdb' where CargoName='Commodities Dry Bulk'
update CargoType set CargoNameForUrl='rf' where CargoName='Refrigerated Food'

update CargoType set CargoNameForUrl='b' where CargoName='Beverages'
update CargoType set CargoNameForUrl='pp' where CargoName='Paper Products'
update CargoType set CargoNameForUrl='u' where CargoName='Utility'
update CargoType set CargoNameForUrl='fs' where CargoName='Farm Supplies'
update CargoType set CargoNameForUrl='construction' where CargoName='Construction'

--add values in servicetypeurl column of servicetypetable
update ServiceType set ServiceTypeForUrl='vans' where [Service Type]='Vans'
update ServiceType set ServiceTypeForUrl='flatbed' where [Service Type]='Flatbed'
	   
update ServiceType set ServiceTypeForUrl='reefer' where [Service Type]='Reefer'
update ServiceType set ServiceTypeForUrl='drybulk' where [Service Type]='Dry Bulk'
update ServiceType set ServiceTypeForUrl='hazmat' where [Service Type]='Hazmat'
update ServiceType set ServiceTypeForUrl='ct' where [Service Type]='Car Transport'
	   
update ServiceType set ServiceTypeForUrl='mh' where [Service Type]='Mobile Homes'
update ServiceType set ServiceTypeForUrl='tankers' where [Service Type]='Tankers'
update ServiceType set ServiceTypeForUrl='intermodal' where [Service Type]='Intermodal'
update ServiceType set ServiceTypeForUrl='livestock' where [Service Type]='Livestock'
update ServiceType set ServiceTypeForUrl='c' where [Service Type]='Coal/Coke'
