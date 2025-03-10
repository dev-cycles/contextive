BUCKET_NAME=($(aws ssm get-parameter --name "/$CONTEXTIVE_STAGE/docs/bucketName" --query Parameter.Value --output text))
SHA=($(git rev-parse --short HEAD))
VERSION=($(jq .version package.json -r))
DOCS_PATH=$VERSION
if [[ "$CONTEXTIVE_STAGE" = "test" ]]; then
    DOCS_PATH=$VERSION+$SHA
fi
export BASE_URL=/ide/$DOCS_PATH
npm run build
S3_URL=s3://$BUCKET_NAME$BASE_URL
echo Deploying to $S3_URL
aws s3 sync dist $S3_URL